using VirtualPM.Models;
using VirtualPM.Services;

namespace VirtualPM.Jobs;

public class CheckDailyUpdatesJob(
    SlackService slackService,
    ClaudeService claudeService,
    AsanaService asanaService,
    IConfiguration configuration)
{
    private readonly List<string> _ignoredMembers = configuration.GetSection("VirtualPM:IgnoredMembersList").Get<List<string>>() ?? [];

    public async Task ExecuteAsync()
    {
        var asanaUsersByEmail = await asanaService.GetAllUsersAsDictionaryAsync();
        List<string> slackMembers = await slackService.GetChannelMembersAsync();
        List<UserReport> allUserReports = [];
        
        foreach (string member in slackMembers)
        {
            if (await slackService.IsBotUserAsync(member) || _ignoredMembers.Contains(member))
            {
                continue;
            }


            (string userName, string userEmail) = await slackService.GetUserInfoAsync(member);
            bool hasMessages = await slackService.UserHasMessagesTodayAsync(member);

            UserReport report = new ()
            {
                SlackUserId = member,
                UserName = userName,
                Email = userEmail,
                PostedToday = hasMessages
            };

            if (!string.IsNullOrEmpty(userEmail))
            {
                if (asanaUsersByEmail.TryGetValue(userEmail, out AsanaUser asanaUser))
                {
                    UserTaskMetrics metrics = await asanaService.GetUserTaskMetricsAsync(
                        asanaUser.Gid,
                        userName,
                        userEmail);
                    report.TaskMetrics = metrics;
                }
            }

            allUserReports.Add(report);
        }

        string claudeMessage = string.Empty;
        if (allUserReports.Count(u => !u.PostedToday) > 0)
        {
            claudeMessage = await claudeService.GenerateHumorousMessageAsync();
        }

        await slackService.SendDailyReportAsync(claudeMessage, allUserReports);
    }
}
