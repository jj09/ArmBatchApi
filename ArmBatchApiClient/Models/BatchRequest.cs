using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ArmBatchApiClient.Models
{
    public class BatchRequest : IEqualityComparer<BatchRequest>
    {
        [JsonProperty(PropertyName = "relativeUrl")]
        public string RelativeUrl { get; private set; }

        [JsonProperty(PropertyName = "httpMethod")]
        public string HttpMethod { get; private set; } = "GET";

        bool IEqualityComparer<BatchRequest>.Equals(BatchRequest x, BatchRequest y)
        {
            return string.Equals(x?.RelativeUrl, y?.RelativeUrl, StringComparison.Ordinal)
                         && string.Equals(x?.HttpMethod, y?.HttpMethod, StringComparison.Ordinal);
        }

        int IEqualityComparer<BatchRequest>.GetHashCode(BatchRequest obj)
        {
            return obj?.RelativeUrl?.GetHashCode() ?? 0;
        }

        public BatchRequest(string relativeUrl)
        {
            RelativeUrl = relativeUrl;
        }
    }
}
