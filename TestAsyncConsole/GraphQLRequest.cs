using Newtonsoft.Json;

namespace TestAsyncConsole
{
    internal class GraphQLRequest
    {
        [JsonProperty("query")]
        public string? Query { get; set; }

        [JsonProperty("variables")]
        public IDictionary<string, object> Variables { get; } = new Dictionary<string, object>();

        public string ToJsonText() =>
            JsonConvert.SerializeObject(this);
    }
}
