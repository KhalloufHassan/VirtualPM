namespace VirtualPM.Models;

public class UserReport
{
    public string SlackUserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool PostedToday { get; set; }
    public UserTaskMetrics TaskMetrics { get; set; }
}
