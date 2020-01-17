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
            Regex alpha = new Regex("[a-z][A-Z]");
            bool quotes = false;
            bool wordStarted = false;
            for(int i = 0; i < commandLength; i++)
            {
                // The command is always first, scan until there is a space
                if (Char.IsLetter(commandString[i]) || (commandString[i] == ' ' && quotes) || (commandString[i] == '\\') || (commandString[i] == ':') || (commandString[i] == '.'))
                {
                    wordStarted = true;
                    s.Append(commandString[i]);
                }
                else if(commandString[i] == ' ' && !quotes && wordStarted)
                {
                    arguments.Add(s.ToString());
                    s.Clear();
                    wordStarted = false;
                }
                else if(commandString[i] == '"' && s.Length == 0)
                {
                    quotes = true;
                }
                else if (quotes && commandString[i] == '"')
                {
                    arguments.Add(s.ToString());
                    s.Clear();
                    quotes = false;
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
                parsed[i] = arguments[i];
            }

            return parsed;
        }
    }
}
