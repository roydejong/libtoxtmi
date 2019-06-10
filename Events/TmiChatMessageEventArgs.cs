using libtoxtmi.Models;
using System;

namespace libtoxtmi.Events
{
    /// <summary>
    /// EventArgs for incoming TMI chat messages.
    /// </summary>
    public class TmiChatMessageEventArgs : EventArgs
    {
        /// <summary>
        /// The parsed TMI chat message.
        /// </summary>
        public TmiChatMessage Message
        {
            get;
            private set;
        }
        
        public TmiChatMessageEventArgs(TmiChatMessage message)
        {
            this.Message = message;
        }
    }
}