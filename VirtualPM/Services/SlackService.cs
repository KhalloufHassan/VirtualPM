using System.Text;
using Slack.NetStandard;
using Slack.NetStandard.WebApi.Chat;
using Slack.NetStandard.WebApi.Conversations;
using Slack.NetStandard.WebApi.Users;
using VirtualPM.Models;

namespace VirtualPM.Services;

public class SlackService
{
    private readonly ISlackApiClient _client;
    private readonly string _channelId;

    public SlackService(IConfiguration configuration)
    {
        string token = configuration["Slack:BotToken"] ?? throw new InvalidOperationException("Slack bot token not configured");
        _channelId = configuration["Slack:ChannelId"] ?? throw new InvalidOperationException("Slack channel ID not configured");
        _client = new SlackWebApiClient(token);
    }

    public async Task<List<string>> GetChannelMembersAsync()
    {
        ConversationMembersResponse response = await _client.Conversations.Members(_channelId);

        if (!response.OK)
        {
            Console.WriteLine($"Error fetching members: {response.Error}");
            return [];
        }

        return response.Members.ToList();
    }

    public async Task<bool> UserHasMessagesTodayAsync(string userId)
    {
        DateTime now = DateTime.UtcNow;
        DateTime startOfDay = new(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        DateTime endOfDay = startOfDay.AddDays(1);

        ConversationHistoryResponse historyResponse = await _client.Conversations.History(new ConversationHistoryRequest
        {
            Channel = _channelId,
            Oldest = ((DateTimeOffset)startOfDay).ToUnixTimeSeconds(),
            Latest = ((DateTimeOffset)endOfDay).ToUnixTimeSeconds(),
            Limit = 1000
        });

        if (!historyResponse.OK)
        {
            Console.WriteLine($"Error fetching history: {historyResponse.Error}");
            return false;
        }

        return historyResponse.Messages.Any(m => m.User == userId);
    }

    public async Task<(string name, string email)> GetUserInfoAsync(string userId)
    {
        UserResponse userResponse = await _client.Users.Info(userId);

        if (!userResponse.OK)
        {
            return (string.Empty, string.Empty);
        }

        string name = userResponse.User.RealName ?? userResponse.User.Name ?? "Unknown";
        string email = userResponse.User.Profile?.Email ?? string.Empty;

        return (name, email);
    }

    public async Task<bool> IsBotUserAsync(string userId)
    {
        UserResponse userResponse = await _client.Users.Info(userId);

        if (!userResponse.OK)
        {
            return true;
        }

        return userResponse.User.IsBot ?? false;
    }

    public async Task SendDailyReportAsync(string claudeMessage, List<UserReport> allUserReports)
    {
        StringBuilder messageBuilder = new();

        // If there are non-posters, tag them with the Claude message
        if (allUserReports.Any(u => !u.PostedToday))
        {
            string tags = string.Join(", ", allUserReports.Where(u => !u.PostedToday).Select(userReport => $"<@{userReport.SlackUserId}>"));
            messageBuilder.AppendLine($"Yo, {tags}");
            messageBuilder.AppendLine(claudeMessage);
        }
        else
        {
            messageBuilder.AppendLine("🎉 *Everyone has posted their daily updates!* 🎉");
        }

        messageBuilder.AppendLine();

        // Build the Asana report table for ALL users
        messageBuilder.AppendLine("📊 *Daily Asana Report*");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine("```");
        messageBuilder.AppendLine($"{"User",-20} | {"Total",6} | {"Overdue",8} | {"Has Task With Future Due Date",28} | {"Sent Daily Update",18}");
        messageBuilder.AppendLine(new string('-', 92));

        foreach (UserReport report in allUserReports)
        {
            string userName = TruncateOrPad(report.UserName, 20);
            string total = report.TaskMetrics?.TotalTaskCount.ToString() ?? "N/A";
            string overdue = report.TaskMetrics?.OverdueTaskCount.ToString() ?? "N/A";
            string hasFuture = report.TaskMetrics?.HasFutureDueDateTasks == true ? "✅" : "❌";
            string posted = report.PostedToday ? "✅" : "❌";

            messageBuilder.AppendLine($"{userName,-20} | {total,6} | {overdue,8} | {hasFuture,28} | {posted,18}");
        }

        messageBuilder.AppendLine("```");

        await PostMessageAsync(messageBuilder.ToString());
    }

    private static string TruncateOrPad(string text, int length)
    {
        if (string.IsNullOrEmpty(text))
            return new string(' ', length);

        if (text.Length > length)
            return text[..(length - 3)] + "...";

        return text.PadRight(length);
    }

    private async Task PostMessageAsync(string message)
    {
        await _client.Chat.Post(new PostMessageRequest
        {
            Channel = _channelId,
            Text = message
        });
    }
}
