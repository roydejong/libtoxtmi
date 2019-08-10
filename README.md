# libtoxtmi
**C# .NET Core library for watching and parsing Twitch Chat via IRC (TMI).**

## About
This library was specifically designed to watch Twitch channels, and parse the TMI messages (Twitch's alternate of the IRC protocol, with some extensions and changes).

🚧 This library is functional, but a work in progress. The feature set is limited. 🚧

### Features
- 👀 Twitch IRC client, with support for watching multiple channels.
- 📖 Reading and parsing TMI messages with all known metadata (bits, badges, subs, etc).

## Installation
To use the library, you must reference it in your project. [This package is available for installation on NuGet](https://www.nuget.org/packages/libtoxtmi):

    PM> Install-Package libtoxtmi -Version 0.1.1
    
Alternatively, you can [download binary releases](https://github.com/roydejong/libtoxtmi/releases) from the repository.

## Basic usage

### Init and auth

Initialize the client with your credentials:

    var client = new TmiChat(twitchUsername, twitchAccessToken);
    
Instead of a password, Twitch IRC uses OAuth access tokens to login. The access token must have the correct permissions to be able to access chat. 

If you do not have an access token, you can generate one here:
https://twitchapps.com/tmi/

### Control and events

Starting and stopping the client is straightforward:

    client.Start(); // Connect to the server and authenticate, start join queue
    client.Stop(); // Close any connections, and shut down the client
    
The client exposes several useful events:

    client.OnStarted += (object sender, EventArgs e) => { /** Connected and authenticated to IRC server **/ };
    client.OnStopped += (object sender, EventArgs e) => { /** Disconnected from server and stopped IRC client **/ };
    client.OnChatMessage += (object sender, TmiChatMessageEventArgs e) => { /** Received a chat message with metadata **/ };
    
Notes when wrangling these events:
- The `OnStarted` event will only trigger once the connection has been established and authentication was successful. Once raised, the client is capable of sending and receiving messages.
- The `OnStopped` event may also be triggered when the connection fails, drops or times out for whatever reason. Once raised, the client is in a state where it can be restarted. 

## Data & Models

### `TmiMessage`

#### Properties

|Property|Type|Description|
|--------|----|-----------|
|`StateProperties`|Dictionary<string, string>|Collection of TMI properties (e.g. raw badge information).|
|`ServerName`|string|The server side identifier for this message. May include user, channel and server name depending on context.|
|`CommandName`|string|The command name, e.g. `PRIVMSG` for a chat message or `ROOMSTATE` for a chat room status message.|
|`ChannelName`|string?|The channel name, if available. The `#` IRC prefix is omitted. May be `NULL` for non-channel-specific messages.|
|`Message`|string?|The message content, if available. The `:` IRC prefix is omitted. May be `NULL` for non-chat messages.|

### `TmiChatMessage` (inherits `TmiMessage`)

#### Properties

|Property|Type|Description|
|--------|----|-----------|
|`UserId`|long|Twitch User ID of sender|
|`UserDisplayName`|string|The sender's display name.|
|`Color`|string|The hex color code for the sender's username in chat.|
|`BitsCheerAmount`|int|The total amount of bits cheered with this message. Set to 0 when this is not a bit cheer message.|
|`IsMod`|bool|Indicates whether or not this message was sent by someone with moderator permissions.|
|`IsAdmin`|bool|If true, sender had an global admin badge.|
|`IsBitsDonator`|bool|If true, sender had a bits cheer badge.|
|`IsBroadcaster`|bool|If true, sender had a broadcaster badge (channel owner).|
|`IsGlobalMod`|bool|If true, sender had a global moderator badge.|
|`IsModerator`|bool|If true, sender had a channel moderator badge.|
|`IsSubscriber`|bool|If true, sender had a channel subscription badge.|
|`IsTurbo`|bool|If true, sender had a Twitch Turbo badge.|
|`IsPartner`|bool|If true, sender had a partner (verified checkmark) badge.|
|`IsPremium`|bool|If true, sender had a ??? Twitch Prime ??? badge.|
|`IsBitsCharityDonator`|bool|If true, sender had a bits cheer badge (charity gift variant).|
|`IsPrivateWhisper`|bool|If true, this was sent to the bot directly as a private message / whisper. (The channel name wil equal the bot username)|
|`IsKnownBot`|bool|If true, this was sent by a known Twitch chat bot.|

#### Methods

|Method|Description|
|------|-----------|
|`public Dictionary<string, int> GetBadgeInfo()`|Gets a list of meta badge information, with the badge name as key and the metadata as value. Badge info is used for details like total subscription length.|
|`public Dictionary<string, int> GetBadges()`|Gets a list of all badges sent with this message, with the badge name as key and the badge version as value. The badge version is the variation of the badge, based on e.g. sub length or bits cheered.|
|`public IEnumerable<string> GetBadgeNames()`|Gets a list of all badge names sent with this message.|
|`public int GetSubscriberBadgeLevel()`|Gets the badge version for the channel sub badge associated with this message. This is related to, but not equal to, the amount of months the user has been subbed.|
|`public int GetMonthsSubscribed()`|Gets the total amount of time this user has been subbed, from the badge info.|
|`public int GetBitsBadgeLevel()`|Gets the badge version for the bits badge associated with this message (cheer value over a period). Returns 0 if the user does not have this badge.|

## Advanced usage

### Parsing a TMI message

You can also use this library directly for its wonderful ability to parse TMI IRC messages:

    var input = "@badge-info=;badges=staff/1,bits/1000;bits=100;color=;display-name=dallas;emotes=;id=b34ccfc7-4977-403a-8a94-33c6bac34fb8;mod=0;room-id=1337;subscriber=0;tmi-sent-ts=1507246572675;turbo=1;user-id=1337;user-type=staff :ronni!ronni@ronni.tmi.twitch.tv PRIVMSG #dallas :cheer100";
    var tmiMessage = (TmiChatMessage)TmiMessageParser.Parse(input);

    Console.WriteLine(tmiMessage.IsStaff); // true
    Console.WriteLine(tmiMessage.IsBitsDonator); // true
    Console.WriteLine(tmiMessage.BitsCheerAmount); // 100
    Console.WriteLine(tmiMessage.GetBitsBadgeLevel()); // 1000
    Console.WriteLine(tmiMessage.Message); // "cheer100"
    
`TmiMessageParser` will return a `TmiMessage` or a more specific subclass if appropriate, depending on the `CommandName`:

|`CommandName`|Maps to|Description|
|-------------|-------|-----------|
|`PRIVMSG`|`TmiChatMessage`|Normal chat message.|
|`WHISPER`|`TmiChatMessage`|Private / whisper message.|
|(default)|`TmiMessage`|Unimplemented commands, e.g. room state messages.|
