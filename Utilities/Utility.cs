using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Reno.Utilities
{
    public abstract class Utility
    {

        public static string[] ParseCommand(string commandString)
        {
            int commandLength = commandString.Length;
            StringBuilder s = new StringBuilder();
            string[] parsed;
            List<string> arguments = new List<string>();
            bool quotes = false;
            bool wordStarted = false;
            for(int i = 0; i < commandLength; i++)
            {
                if (commandString[i] == '"' && s.Length == 0)    // Look for quotes at the beginning of the word
                {
                    quotes = true;
                }
                // The command is always first, scan until there is a space
                if (Char.IsLetterOrDigit(commandString[i]) || Char.IsPunctuation(commandString[i]) || 
                    (commandString[i] == ' ' && quotes) || (commandString[i] == '\\') || (commandString[i] == ':') || 
                    (commandString[i] == '.'))
                {
                    wordStarted = true;
                    s.Append(commandString[i]);
                }
                if(i < commandLength - 1)
                {
                    if(commandString[i] == '"' && commandString[i+1] == ' ')
                    {
                        // End of quoted token
                        quotes = false;
                    }
                }
                if(commandString[i] == ' ' && !quotes && wordStarted)
                {
                    arguments.Add(s.ToString());
                    s.Clear();
                    wordStarted = false;
                }
                if (i == commandLength - 1 && wordStarted)
                {
                    arguments.Add(s.ToString());
                    s.Clear();
                    wordStarted = false;
                }
            }

            parsed = new string[arguments.Count];
            for(int i = 0; i < arguments.Count; i++)
            {
                parsed[i] = arguments[i].Replace("\"", "");
            }

            return parsed;
        }
    }
}
