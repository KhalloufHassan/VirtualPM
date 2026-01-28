using VirtualPM.Models;
using VirtualPM.Services;

namespace VirtualPM.Jobs;

public class CheckDailyUpdatesJob(
    SlackService slackService,
    IMessageGenerator messageGenerator,
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
            if (_ignoredMembers.Contains(member))
            {
                continue;
            }

            // Fetch all user info in a single API call
            SlackUserInfo userInfo = await slackService.GetSlackUserInfo(member);

            // Skip if we couldn't get user info, if user is a bot, or if user is on leave
            if (userInfo == null || userInfo.IsBot || userInfo.IsOnLeave)
            {
                continue;
            }

            bool hasMessages = await slackService.UserHasMessagesTodayAsync(member);

            UserReport report = new ()
            {
                SlackUserId = member,
                UserName = userInfo.Name,
                Email = userInfo.Email,
                PostedToday = hasMessages
            };

            if (!string.IsNullOrEmpty(userInfo.Email))
            {
                if (asanaUsersByEmail.TryGetValue(userInfo.Email, out AsanaUser asanaUser))
                {
                    UserTaskMetrics metrics = await asanaService.GetUserTaskMetricsAsync(
                        asanaUser.Gid,
                        userInfo.Name,
                        userInfo.Email);
                    report.TaskMetrics = metrics;
                }
            }

            allUserReports.Add(report);
        }

        string humorousMessage = string.Empty;
        if (allUserReports.Count(u => !u.PostedToday) > 0)
        {
            humorousMessage = await messageGenerator.GenerateHumorousMessageAsync();
        }

        await slackService.SendDailyReportAsync(humorousMessage, allUserReports);
    }
}
