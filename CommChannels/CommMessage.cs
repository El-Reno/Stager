using System;
using System.Collections.Generic;
using System.Text;

namespace Reno.Comm
{
    public class CommMessage
    {
        CommandHeader header;
        char[] message;

        /// <summary>
        /// Constructor for CommMessage. This is a wrapper for a char[] array
        /// </summary>
        /// <param name="message">Message, char[]</param>
        /// <param name="header">Header for the communications message</param>
        public CommMessage(CommandHeader header, char[] message)
        {
            this.header = header;
            this.message = message;
        }

        /// <summary>
        /// Returns the message as a char[]
        /// </summary>
        public char[] Message
        {
            get
            {
                return message;
            }
        }
    }
}
