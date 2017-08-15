using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ArmBatchApiClient.Models
{
    public class PendingRequest
    {
        public TaskCompletionSource<JObject> TaskCompletionSource
        {
            get;
            private set;
        }

        public Stopwatch Stopwatch
        {
            get;
            private set;
        }

        public PendingRequest()
        {
            TaskCompletionSource = new TaskCompletionSource<JObject>();
            Stopwatch = Stopwatch.StartNew();
        }
    }
}
