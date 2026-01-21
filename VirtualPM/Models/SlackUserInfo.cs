namespace VirtualPM.Models;

public class SlackUserInfo
{
    public string UserId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsBot { get; set; }
    public bool IsOnLeave { get; set; }
}