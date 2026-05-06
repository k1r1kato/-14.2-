using System.Text.Json.Serialization;

namespace SimpleApiServer
{
    public class TaskItem
    {
        [JsonPropertyName("id")] public int Id { get; set; }
        [JsonPropertyName("title")] public string Title { get; set; }
        [JsonPropertyName("priority")] public int Priority { get; set; }
    }
}