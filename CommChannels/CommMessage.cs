﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Reno.Comm
{
    public class CommMessage
    {
        CommHeader header;
        byte[] message;

        /// <summary>
        /// Constructor for CommMessage. This is a wrapper for a char[] array
        /// </summary>
        /// <param name="message">Message, char[]</param>
        /// <param name="header">Header for the communications message</param>
        public CommMessage(CommHeader header, byte[] message)
        {
            this.header = header;
            this.message = message;
        }

        /// <summary>
        /// Returns the message data as a byte[]
        /// </summary>
        public byte[] Message
        {
            get
            {
                return message;
            }
        }
        /// <summary>
        /// Returns the message data as a string
        /// </summary>
        public string GetMessageString
        {
            get
            {
                return Encoding.UTF8.GetString(message);
            }
        }
    }
}
