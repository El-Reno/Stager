using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace InjectTest
{
    public class Tester
    {
        public static void Execute()
        {
            Console.WriteLine("This is a test string from a loaded DLL");
            string command = @"/C tree C:\ /F /A";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = command;
            //p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();
            Console.WriteLine(output);
        }
        /// <summary>
        /// This function is called by the requestor to recursively look through the supplied directory
        /// </summary>
        /// <param name="dir">The directory to begin looking through</param>
        /// <param name="format">The output format for the directory traversal. Accepted formats are: ASCII, JSON, or XML</param>
        /// <returns></returns>
        public static string EnumerateDirectoryStructure(string dir, string format)
        {
            format = format.ToUpper();
            if (format.Contains("ASCII"))
                return EnumerateDirectoryStructureASCII(dir, 0);
            else if (format.Contains("JSON"))
                return "";
            else if (format.Contains("XML"))
                return "";
            else
                return "";
        }

        private static string EnumerateDirectoryStructureASCII(string dir, int l)
        {
            string level = "";
            for (int i = 0; i < l; i++)
                level += "-";
            level += " ";
            string structure = level + dir + "\n";
            DirectoryInfo info = new DirectoryInfo(dir);
            foreach (DirectoryInfo i in info.EnumerateDirectories())
            {
                structure += "|" + EnumerateDirectoryStructureASCII(i.FullName, l+1);
                try
                {
                    foreach (string file in Directory.EnumerateFiles(i.FullName))
                    {
                        structure += String.Format("|{0}{1}", level, file) + "\n";
                    }
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine("Denied Access");
                }
            }
            return structure;
        }

        public static void ExecuteAnother(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
