# libtoxtmi
C# .NET Core library for watching and parsing Twitch Chat via IRC (TMI).

## About
This library was specifically designed to watch Twitch channels, and parse the TMI messages (Twitch's alternate of the IRC protocol, with some extensions and changes).

🚧 This library is functional, but a work in progress. The feature set is limited. 🚧

### Features
- Twitch IRC client, with support for watching multiple channels.
- Reading and parsing TMI messages with all known metadata (bits, badges, subs, etc).

## Installation
To use the library, you must reference it in your project. [This package is available for installation on NuGet](https://www.nuget.org/packages/libtoxtmi):

    PM> Install-Package libtoxtmi -Version 0.1.0  
    
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
