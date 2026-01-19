using System.Text.Json.Serialization;

namespace VirtualPM.Models;

public class AsanaTask
{
    [JsonPropertyName("gid")]
    public string Gid { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("due_on")]
    public DateTime? DueOn { get; set; }
}
