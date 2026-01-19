namespace VirtualPM.Models;

public class UserTaskMetrics
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasFutureDueDateTasks { get; set; }
    public int OverdueTaskCount { get; set; }
    public int TotalTaskCount { get; set; }
}
