using System;

namespace libtoxtmi.Tmi
{
    /// <summary>
    /// Utility for quickly reading parts of raw TMI/IRC messages.
    /// </summary>
    public class TmiIrcReader
    {
        /// <summary>
        /// Index position.
        /// </summary>
        private int index;
        /// <summary>
        /// String buffer, raw IRC message value.
        /// </summary>
        private string buffer;

        /// <summary>
        /// Constructs a reader from the raw TMI/IRC message.
        /// </summary>
        public TmiIrcReader(string buffer)
        {
            this.index = 0;
            this.buffer = buffer;
        }

        /// <summary>
        /// Resets the read pointer.
        /// </summary>
        public void Reset()
        {
            this.index = 0;
        }

        /// <summary>
        /// Advances the read pointer.
        /// </summary>
        public void Skip(int length)
        {
            this.index += length;
        }

        /// <summary>
        /// Reads a substring directly from the start of the buffer.
        /// </summary>
        /// <remarks>
        /// This method ignores, and does not advance, the read pointer.
        /// </remarks>
        public string Extract(int index, int length)
        {
            return this.buffer.Substring(index, length);
        }

        /// <summary>
        /// Reads the next part of the buffer, advancing the read pointer.
        /// </summary>
        /// <remarks>
        /// This method applies and advances the read pointer.
        /// </remarks>
        public TmiIrcReader ReadNext(int length)
        {
            var result = Extract(this.index, length);
            Skip(length);
            return new TmiIrcReader(result);
        }

        /// <summary>
        /// Reads the next part of the buffer, up to a delimiter character.
        /// </summary>
        /// <remarks>
        /// This method applies and advances the read pointer.
        /// </remarks>
        public TmiIrcReader ReadNextUntil(char delimiter, bool stripDelim = true)
        {
            var nextIndex = this.buffer.IndexOf(delimiter, this.index);

            if (nextIndex == -1)
                return new TmiIrcReader("");

            var readLength = (nextIndex - this.index);

            if (!stripDelim)
                readLength += 1;

            var result = Extract(this.index, readLength);
            Skip(stripDelim ? (readLength + 1) : readLength);
            return new TmiIrcReader(result);
        }

        /// <summary>
        /// Reads any remaining text in the buffer.
        /// </summary>
        /// <remarks>
        /// This method applies and advances the read pointer.
        /// </remarks>
        public TmiIrcReader ReadRemainder()
        {
            var result = buffer.Substring(this.index);
            this.index = (buffer.Length - 1);
            return new TmiIrcReader(result);
        }

        /// <summary>
        /// Gets whether or not the pointer is currently at the start of the buffer (reset).
        /// </summary>
        public bool AtStart
        {
            get
            {
                return this.index == 0;
            }
        }

        /// <summary>
        /// Gets whether or not the pointer has reached the end of the buffer.
        /// </summary>
        public bool ReachedEnd
        {
            get
            {
                return this.index >= (this.buffer.Length - 1);
            }
        }

        /// <summary>
        /// Gets whether or not the string buffer is NULL or an empty string.
        /// </summary>
        public bool IsNullOrEmpty
        {
            get
            {
                return String.IsNullOrEmpty(this.buffer);
            }
        }

        /// <summary>
        /// Implicit conversation from string to reader instance.
        /// </summary>
        /// <remarks>
        /// This method ignores, and does not advance, the read pointer.
        /// </remarks>
        public static implicit operator TmiIrcReader(string value)
        {
            return new TmiIrcReader(value);
        }

        /// <summary>
        /// Implicit conversation from reader instance to string.
        /// </summary>
        /// <remarks>
        /// This method ignores, and does not advance, the read pointer.
        /// </remarks>
        public static implicit operator string(TmiIrcReader value)
        {
            return value.ToString();
        }

        /// <summary>
        /// Returns the raw buffer contents.
        /// </summary>
        /// <remarks>
        /// This method ignores, and does not advance, the read pointer.
        /// </remarks>
        public override string ToString()
        {
            return this.buffer;
        }
    }
}