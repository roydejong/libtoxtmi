using System.Collections.Generic;

namespace libtoxtmi.Models
{
    /// <summary>
    /// Represents TMI message data.
    /// </summary>
    public class TmiMessage
    {
        #region Fields
        /// <summary>
        /// TMI properties, prefixed to the front of the messages.
        /// Contains metadata such as user badges, persmissions, sub and bit info, etc.
        /// The properties changed depending on the CommandName.
        /// </summary>
        public Dictionary<string, string> StateProperties = new Dictionary<string, string>();

        /// <summary>
        /// The server-side IRC identifier for this user.
        /// Contains information on the server, channel, and sender (to the extent available).
        /// </summary>
        /// <remarks>
        /// The ":" prefix is automatically omitted for this value.
        /// </remarks>
        /// <example>ronni!ronni@ronni.tmi.twitch.tv</example>
        public string ServerName = "anon!anon@anon.tmi.twitch.tv";

        /// <summary>
        /// The command / op name for this message.
        /// </summary>
        /// <example>PRIVMSG</example>
        public string CommandName = "???";

        /// <summary>
        /// The name of the channel.
        /// </summary>
        /// <remarks>
        /// The "#" prefix is automatically omitted for this value.
        /// May be set to NULL if this TmiMessage does not have a channel name (e.g. global user state).
        /// </remarks>
        public string ChannelName = null;

        /// <summary>
        /// The raw message content, if available.
        /// </summary>
        /// <remarks>
        /// The ":" prefix is automatically omitted for this value.
        /// May be set to NULL if this TmiMessage does not have a message body (e.g. global user or room state updates).
        /// </remarks>
        public string Message = null;
        #endregion

        #region Property helpers
        public string GetStateProp(string key, string defaultValue = null)
        {
            if (StateProperties.ContainsKey(key))
                return StateProperties[key];
            return defaultValue;
        }
        #endregion
    }
}