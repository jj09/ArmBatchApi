using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ArmBatchApiClient.Models
{
    public class Resource
    {
        private string _mergedTypeName;

        [JsonProperty]
        private string _subscriptionName;

        [JsonProperty]
        private string _type;

        public Resource()
        {
            Tags = new Dictionary<string, string>();
        }

        /// <summary>
        /// The id of the resource.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets the identity of the resource.
        /// </summary>
        public IDictionary<string, object> Identity { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The kind of resource.
        /// </summary>
        /// <value>The kind.</value>
        public string Kind { get; set; }

        /// <summary>
        /// The location.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// The ID of the resource that manages this resource.
        /// </summary>
        public string ManagedBy { get; set; }

        /// <summary>
        /// The name of the resource.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a JSON object representing the original content.
        /// </summary>
        /// <value>A JSON object representing original content.</value>
        public JObject OriginalContent { get; set; } = new JObject();

        /// <summary>
        /// Gets the plan of the resource.
        /// </summary>
        public IDictionary<string, object> Plan { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the SKU of the resource.
        /// </summary>
        public IDictionary<string, object> Sku { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The resource tags
        /// </summary>
        public IDictionary<string, string> Tags { get; set; }

        /// <summary>
        /// Gets the collection of properties for the resource item.
        /// </summary>
        public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

        public override bool Equals(object obj)
        {
            var resource = obj as Resource;
            return resource != null
                && Id != null
                && Id.Equals(resource.Id, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Id;
        }
    }
}
