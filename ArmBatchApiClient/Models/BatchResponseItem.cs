using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace ArmBatchApiClient.Models
{
    public class BatchResponseItem<T>
    {
        [JsonProperty(PropertyName = "content")]
        public T Content { get; private set; }

        [JsonProperty(PropertyName = "headers")]
        public IReadOnlyDictionary<string, string> Headers { get; private set; }

        [JsonProperty(PropertyName = "httpStatusCode")]
        public HttpStatusCode HttpStatusCode { get; private set; }
    }
}
