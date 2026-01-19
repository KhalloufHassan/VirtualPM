using Anthropic.SDK;
using Anthropic.SDK.Constants;
using Anthropic.SDK.Messaging;

namespace VirtualPM.Services;

public class ClaudeService
{
    private readonly AnthropicClient _client;

    public ClaudeService(IConfiguration configuration)
    {
        var apiKey = configuration["Anthropic:ApiKey"]
            ?? throw new InvalidOperationException("Anthropic API key not configured");
        _client = new AnthropicClient(new APIAuthentication(apiKey));
    }

    public async Task<string> GenerateHumorousMessageAsync()
    {
        const string systemPrompt = @"You are a witty virtual project manager who sends humorous reminders to developers who forgot to post their daily work updates in Slack.

Generate a single funny, coding-related insult or sarcastic remark directed at a group of people who missed their daily update. The message should:
- Be supper aggressive or passive aggressive, but never inappropriate or mean
- Reference programming concepts, coding practices, or developer culture
- Use Arabic only when you are told to, English otherwise
- Keep it concise (1-2 sentences max)
- Address the group collectively (not individuals)
- Make them laugh while reminding them to update their work

Examples of the tone:
- 'Your code may compile, but your communication skills are throwing exceptions.'
- 'If updating your tasks was an API call, we'd all be timing out by now.'
- 'حتى القهوة سئمت انتظاركم لتبدأوا بكتابة التحديث.'
- 'Looks like someone forgot that git commit isn't the only way to document your work.'
- Don't mention anything other than the sentence like here you go and colons
";

        string userPrompt = $"Generate a humorous coding-related message for developers who forgot to post their daily updates";
        if (Random.Shared.Next(2) == 0)
        {
            userPrompt += " (Use Arabic)";
        }

        var messages = new List<Message>
        {
            new(RoleType.User, userPrompt)
        };

        MessageParameters parameters = new ()
        {
            Messages = messages,
            MaxTokens = 150,
            Model = AnthropicModels.Claude45Haiku,
            Stream = false,
            Temperature = 1.0m,
            System = [new SystemMessage(systemPrompt)]
        };

        MessageResponse response = await _client.Messages.GetClaudeMessageAsync(parameters);

        TextContent textContent = response.Content.FirstOrDefault() as TextContent;
        return textContent?.Text?.Trim() ?? "You haven't updated your work today. Please do so!";
    }
}
