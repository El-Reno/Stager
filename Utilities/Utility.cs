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

        /// <summary>
        /// Helper function to print out download progress bar
        /// Shows percentage based on 0-100
        /// </summary>
        /// <param name="bytesRead">Bytes read of the file</param>
        /// <param name="downloadSize">Size of the file</param>
        /// <param name="fileName">Name of file</param>
        public static void DownloadStatus(long bytesRead, long downloadSize, string fileName)
        {
            // Get the current console BufferWidth
            int width = Console.BufferWidth;
            double percentComplete = Math.Round(((double)bytesRead / (double)downloadSize) * 100);
            string progressBeginning = "Downloading " + fileName + " |";
            string progressEnd = "| " + percentComplete.ToString() + "%";
            // How much screen buffer have we used so far
            int bufferRemaining = width - (progressBeginning.Length + progressEnd.Length);
            int numEquals = (int)Math.Round((percentComplete / 100) * (double)bufferRemaining);
            string equalSigns = new String('=', numEquals);
            bufferRemaining -= equalSigns.Length;
            string spaces = new string(' ', bufferRemaining);
            string progress = progressBeginning + equalSigns + spaces + progressEnd;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(progress);
        }
    }
}
