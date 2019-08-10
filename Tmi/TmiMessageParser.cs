using libtoxtmi.Models;
using System.Collections.Generic;

namespace libtoxtmi.Tmi
{
    /// <summary>
    /// Utility for parsing TMI messages.
    /// </summary>
    public static class TmiMessageParser
    {
        /// <summary>
        /// Parses a Twitch Chat TMI message, from the raw IRC message.
        /// </summary>
        /// <param name="raw">The raw IRC messages, as received on the wire.</param>
        /// <returns>Parsed message data, either as an instance of TmiMessage, or a more specific model subtype if appropriate.</returns>
        public static TmiMessage Parse(string raw)
        {
            /**
             * // On room join, we get a USERSTATE message for ourselves:
             * 
             * @badge-info=;badges=;color=;display-name=Toxmeter;emote-sets=0;mod=0;subscriber=0;user-type= :tmi.twitch.tv USERSTATE #kinamazing
             * @badge-info=;badges=;color=;display-name=Toxmeter;emote-sets=0;mod=0;subscriber=0;user-type= :tmi.twitch.tv USERSTATE #vader
             * 
             * // On room join, we also get a ROOMSTATE message:
             * 
             * @emote-only=0;followers-only=0;r9k=0;rituals=0;room-id=90091619;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #kinamazing
             * @emote-only=0;followers-only=10;r9k=0;rituals=0;room-id=69759951;slow=0;subs-only=0 :tmi.twitch.tv ROOMSTATE #vader
             * 
             * // Regular messages are also prefixed with their full USERSTATE:
             * 
             * @badge-info=subscriber/3;badges=subscriber/3;color=#1959B3;display-name=Crtka;emote-only=1;emotes=9:0-1;flags=;id=d4d8826d-0ac0-4966-b58a-830353b3a56a;mod=0;room-id=90091619;subscriber=1;tmi-sent-ts=1560102937011;turbo=0;user-id=180341394;user-type= :crtka!crtka@crtka.tmi.twitch.tv PRIVMSG #kinamazing :<3
             * 
             * // The IRC library does not understand this, so:
             * 
             * e.Message.Command == The full @badge-info[...] block
             * e.Message.Parameters[0] == The IRC user identifier e.g. ":crtka!crtka@crtka.tmi.twitch.tv"
             * e.Message.Parameters[1] == command "PRIVMSG"
             * e.Message.Parameters[2] == channel "#kinamazing"
             * e.Message.Parameters[3] == just the basic plain old message no markers "<3"
             * 
             * // What a WHISPER looks like:
             * 
             * @badges=twitchcon2018/1;color=#FF69B4;display-name=AFluHatinRapper;emotes=;message-id=19;thread-id=63469880_248897310;turbo=0;user-id=63469880;user-type= :afluhatinrapper!afluhatinrapper@afluhatinrapper.tmi.twitch.tv WHISPER toxmeter :test 123
             */

            var reader = new TmiIrcReader(raw);

            // Step 1: Read the state prefix, @-prefixed properties
            var stateProperties = new Dictionary<string, string>();
            var hasStatePrefix = raw.StartsWith("@");

            if (hasStatePrefix)
            {
                var stateReader = reader.ReadNextUntil(' ');
                stateReader.Skip(1); // @

                do
                {
                    var nextPropKv = stateReader.ReadNextUntil(';', true);

                    if (nextPropKv.IsNullOrEmpty)
                        break;

                    var key = nextPropKv.ReadNextUntil('=');
                    var value = nextPropKv.ReadRemainder();

                    stateProperties[key] = value;
                }
                while (true);
            }

            // Step 2: Read server name, channel name, and command name
            string serverName = reader.ReadNextUntil(' ');
            string commandName = reader.ReadNextUntil(' ');
            string channelName = null;
            string message = null;
            bool isWhisper = false;

            switch (commandName)
            {
                case "GLOBALUSERSTATE":
                    // No message, no channel
                    break;
                case "ROOMSTATE":
                case "USERSTATE":
                    // No message, but has channel
                    channelName = reader.ReadRemainder();
                    break;
                case "PRIVMSG":
                default:
                    // Has both message and channel
                    channelName = reader.ReadNextUntil(' ');
                    message = reader.ReadRemainder();
                    break;
                case "WHISPER":
                    // In case of whispers, the channel is your own username w/o # prefix
                    isWhisper = true;
                    channelName = reader.ReadNextUntil(' ');
                    message = reader.ReadRemainder();
                    break;
            }

            // Step 3: Clean up
            if (channelName != null && channelName.Length > 1 && !isWhisper)
                channelName = channelName.ToString().Substring(1); // Skip "#"

            if (serverName != null && serverName.Length > 1)
                serverName = serverName.ToString().Substring(1); // Skip ":"

            if (message != null && message.Length > 1)
                message = message.ToString().Substring(1); // Skip ":"

            // Step 4: Factory 
            TmiMessage result;

            if (commandName == "PRIVMSG" || commandName == "WHISPER")
            {
                result = new TmiChatMessage();
            }
            else
            {
                result = new TmiMessage();
            }

            result.ChannelName = channelName;
            result.CommandName = commandName;
            result.Message = message;
            result.ServerName = serverName;
            result.StateProperties = stateProperties;
            result.IsPrivateWhisper = isWhisper;
            return result;
        }
    }
}