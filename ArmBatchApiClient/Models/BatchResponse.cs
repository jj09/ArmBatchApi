using System;
using Newtonsoft.Json;

namespace ArmBatchApiClient.Models
{
    public class BatchResponse<T>
    {
        [JsonProperty(PropertyName = "responses")]
        public BatchResponseItem<T>[] Responses { get; private set; }
    }
}
