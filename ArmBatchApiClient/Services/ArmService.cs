using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ArmBatchApiClient.Helpers;
using ArmBatchApiClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArmBatchApiClient.Services
{
    public class ArmService
    {
        private readonly HttpClient _httpClient;

        private readonly ConcurrentDictionary<BatchRequest, PendingRequest> _pendingRequestsMap;
        private readonly ConcurrentQueue<BatchRequest> _pendingRequestsQueue;
        private readonly OneShotTimer _batchTaskTimer;

        private const string BatchApiUri = "https://management.azure.com/batch?api-version=2015-11-01";
        private const string ApiVersion = "2015-05-01";

        private readonly string _armToken;
        private readonly int _batchSize;
        private readonly int _batchDelayMs;

        private IDictionary<string, string> _armHeaders;
        private IDictionary<string, string> ArmHeaders
        {
            get
            {
                if (_armHeaders == null)
                {
                    _armHeaders = new Dictionary<string, string>
                    {
                        { "Authorization", _armToken }
                    };
                }

                return _armHeaders;
            }
        }

        public int BatchRequestsCount { get; private set; }

        public ArmService(string armToken, int batchSize, int batchDelayMs)
        {
            _armToken = armToken;
            _batchSize = batchSize;
            _batchDelayMs = batchDelayMs;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;

            _pendingRequestsMap = new ConcurrentDictionary<BatchRequest, PendingRequest>();
            _pendingRequestsQueue = new ConcurrentQueue<BatchRequest>();
            _batchTaskTimer = new OneShotTimer(
                TimeSpan.FromMilliseconds(_batchDelayMs),
                BatchRequestDipatcher);
        }

        public async Task<Resource> GetResource(string resourceId)
        {
            try
            {
                return await EnqueueBatchRequest<Resource>(resourceId);
            }
            catch (Exception)
            {
                return new Resource()
                {
                    Id = resourceId
                };
            }
        }

        private async Task<T> EnqueueBatchRequest<T>(string path)
        {
            Console.WriteLine($"---ENQUEUE Batch Request: {path}");

            var uriBuilder = new UriBuilder
            {
                Path = path,
                Query = $"api-version={ApiVersion}"
            };

            var request = new BatchRequest(uriBuilder.Uri.PathAndQuery);
            var pendingRequest = _pendingRequestsMap.GetOrAdd(request, unused => new PendingRequest());

            _pendingRequestsQueue.Enqueue(request);

            if (_pendingRequestsQueue.Count < _batchSize)
            {
                _batchTaskTimer.Reset();
            }

            var result = await pendingRequest.TaskCompletionSource.Task;

            return result.ToObject<T>();
        }

        private async Task BatchRequestDipatcher()
        {
            try
            {
                while (true)
                {
                    var pendingRequests = new List<BatchRequest>();
                    var pendingTasks = new List<PendingRequest>();

                    while (_pendingRequestsQueue.Count > 0 && pendingRequests.Count < _batchSize)
                    {
                        if (_pendingRequestsQueue.TryDequeue(out BatchRequest request))
                        {
                            PendingRequest pendingRequest;
                            if (_pendingRequestsMap.TryRemove(request, out pendingRequest))
                            {
                                pendingRequests.Add(request);
                                pendingTasks.Add(pendingRequest);
                            }
                        }
                    }

                    if (pendingRequests.Count == 0)
                    {
                        break;
                    }

                    var body = new { requests = pendingRequests };

                    Console.WriteLine($"---SENDING Batch Request (size = {pendingRequests.Count})...");
                    ++BatchRequestsCount;

                    var result = await SendPostRequest<BatchResponse<JObject>>(
                        new Uri(BatchApiUri),
                        SerializeObject(body));

                    for (var i = 0; i < pendingRequests.Count; ++i)
                    {
                        var taskCompletionSource = pendingTasks[i].TaskCompletionSource;
                        var httpStatusCode = result.Responses[i].HttpStatusCode;

                        if (httpStatusCode < HttpStatusCode.OK || httpStatusCode >= HttpStatusCode.MultipleChoices)
                        {
                            taskCompletionSource.SetException(new Exception($"Batch Request returned invalid status: {httpStatusCode.ToString()}"));
                        }
                        else
                        {
                            taskCompletionSource.SetResult(result.Responses[i].Content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task<T> SendPostRequest<T>(Uri uri, string body)
        {
            using (var response = await RequestWithBody(HttpMethod.Post, uri, body, ArmHeaders))
            {
                if (response == null || !response.IsSuccessStatusCode)
                {
                    // 306/Unused when the response is null
                    throw new Exception("Error");
                }

                if (typeof(T).Equals(typeof(MemoryStream)))
                {
                    var stream = new MemoryStream();
                    await response.Content.CopyToAsync(stream);
                    return (T)(object)stream;
                }

                var content = response?.Content != null ? await response.Content.ReadAsStringAsync() : null;

                if (typeof(T).Equals(typeof(string)))
                {
                    return (T)(object)content;
                }

                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        private async Task<HttpResponseMessage> RequestWithBody<T>(HttpMethod method, Uri uri, T body, IDictionary<string, string> headers = null)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                if (headers != null)
                {
                    foreach (var keyVal in headers)
                    {
                        request.Headers.Add(keyVal.Key, new[] { keyVal.Value });
                    }
                }

                string stringContent = typeof(T) != typeof(string)
                    ? JsonConvert.SerializeObject(body)
                    : (string)(object)body;

                request.Content = new StringContent(stringContent, Encoding.UTF8, "application/json");

                return await _httpClient.SendAsync(request);
            }
        }

        private static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(
                value,
                new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
               );
        }
    }
}
