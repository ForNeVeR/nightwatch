<!--
SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Notification Providers
======================

Nightwatch uses _notification providers_ to alert administrators when resource checks fail or recover. Each notification provider type has its own configuration parameters.

Notification providers are configured in the notification directory specified in your `nightwatch.yml`. Each `*.yml` file in that directory describes a notification provider.

## Telegram Notification

Sends notifications via a Telegram bot.

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: myNotifications/telegram # notification provider identifier
type: telegram
param:
    bot-token: YOUR_BOT_TOKEN_HERE # Telegram bot API token
    chat-id: YOUR_CHAT_ID_HERE # target chat ID for notifications
```

### Parameters

- `bot-token`: your Telegram bot API token (obtain from [@BotFather](https://t.me/BotFather))
- `chat-id`: the target chat ID where notifications will be sent

### Setup Instructions

1. Create a bot via [@BotFather](https://t.me/BotFather) on Telegram
2. Copy the bot token provided by BotFather
3. Start a chat with your bot and send any message (or add it to a shared channel / chat)
4. Get your chat id (e.g., by visiting `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates`)
5. Configure the notification provider with your bot token and chat ID

