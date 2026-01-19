namespace VirtualPM.Models;

public class AsanaWorkspace
{
    [System.Text.Json.Serialization.JsonPropertyName("gid")]
    public string Gid { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}