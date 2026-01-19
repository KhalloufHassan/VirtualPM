# Virtual PM 🤖

An intelligent Bot that monitors daily work updates, generates humorous reminders using AI, and tracks task metrics from Asana.

## Features

- **Automated Daily Monitoring**: Checks Slack channels for user updates at a scheduled time
- **Daily Message**: Sends ONE consolidated message per day
- **AI-Powered Group Messages**: Generates contextual, humorous coding-related messages using AI API that tag all who didn't send daily updates
- **Comprehensive Asana Report**: Displays a formatted table for ALL channel users showing:
  - Total incomplete tasks
  - Overdue task count
  - Future due date tasks indicator (Yes/No)
  - Whether they posted their Slack update today (Yes/No)
- **User Mapping**: Automatically matches Slack users to Asana users by email

## Setup Instructions

### Prerequisites

- Slack workspace with bot permissions
- Anthropic API key
- Asana access token

### 1. Slack Bot Setup

1. Go to [api.slack.com/apps](https://api.slack.com/apps)
2. Create a new app or use existing
3. Required Bot Token Scopes:
   - `channels:history` - Read channel messages
   - `channels:read` - View channel info
   - `chat:write` - Post messages
   - `users:read` - Get user information
   - `users:read.email` - Access user emails
4. Install app to workspace
5. Copy the **Bot User OAuth Token** (starts with `xoxb-`)
6. Invite bot to your target channel: `/invite @YourBotName`
7. Copy the **Channel ID** (from channel details or URL)

### 2. Anthropic API Key

1. Sign up at [console.anthropic.com](https://console.anthropic.com)
2. Navigate to API Keys
3. Create a new API key
4. Copy the key (starts with `sk-ant-`)

### 3. Asana Access Token

1. Log in to Asana
2. Go to [app.asana.com/0/developer-console](https://app.asana.com/0/developer-console)
3. Create a Personal Access Token
4. Copy the token

### 4. Configuration

Edit `appsettings.json`:

```json
{
  "Slack": {
    "BotToken": "xoxb-YOUR-SLACK-BOT-TOKEN",
    "ChannelId": "C03A5GA1674"
  },
  "Anthropic": {
    "ApiKey": "sk-ant-YOUR-ANTHROPIC-API-KEY"
  },
  "Asana": {
    "AccessToken": "YOUR-ASANA-ACCESS-TOKEN"
  }
}
```

### 5. Customize Ignored Users

```json
{
  "VirtualPM": {
    "IgnoredMembersList": []
  }
}
```

To find a user's ID: Click their profile in Slack → More → Copy member ID

### 6. Adjust Schedule

The job runs at 1 PM Monday-Friday by default. To change, edit the cron expression in `appsettings.json`:

```json
{
  "VirtualPM": {
    "CronSchedule": "0 13 * * 0-4"
  }
}
```