using System;
using System.IO;
using System.Diagnostics;
using System.Xml;
using System.Text;

namespace Reno.Stages
{
    public class DirectoryTraversal
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
        /// This function calls one of several helper functions depending on the formate requested for a directory structure enumeration
        /// </summary>
        /// <param name="dir">The starting directory to enumerate from</param>
        /// <param name="format">A format of the output - ASCII or XML</param>
        /// <returns></returns>
        public static string EnumerateDirectoryStructure(string dir, string format)
        {
            string directory = "";
            try
            {
                if (format.Contains("TEXT"))
                    directory = EnumerateDirectoryStructureText(dir, 0);
                if (format.Contains("XML"))
                {
                    StringBuilder xmlStringBuilder = new StringBuilder();
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.IndentChars = "\t";
                    XmlWriter xmlWriter = XmlWriter.Create(xmlStringBuilder, settings);
                    EnumerateDirectoryStructureXML(dir, ref xmlWriter);
                    directory = xmlStringBuilder.ToString();
                    xmlWriter.Close();
                }
            }
            catch (InvalidOperationException inv)
            {
                Console.WriteLine("Invalid operation: " + inv.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }

            return directory;
        }
        /// <summary>
        /// This function enumerates the entire directory structure from a given starting point and outputs it in text string format
        /// </summary>
        /// <param name="dir">The starting directory to recursively enumerate as a string</param>
        /// <param name="l">The level the current directory is at - used for recursive purposes</param>
        /// <returns>A string of the directory structure</returns>
        private static string EnumerateDirectoryStructureText(string dir, int l)
        {
            string level = "";
            for (int i = 0; i < l; i++)
                level += "-";
            level += " ";
            string structure = level + dir + "\n";
            DirectoryInfo info = new DirectoryInfo(dir);
            foreach (DirectoryInfo i in info.EnumerateDirectories())
            {
                structure += "|" + EnumerateDirectoryStructureText(i.FullName, l+1);
            }
            try
            {
                foreach (string file in Directory.EnumerateFiles(dir))
                {
                    structure += String.Format("|{0}{1}", level, file) + "\n";
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Denied Access");
            }
            return structure;
        }
        /// <summary>
        /// This function enumerates the entire directory structure from a given starting point and outputs it in XML format
        /// </summary>
        /// <param name="dir">The starting directory to recursively enumerate as a string</param>
        /// <param name="writer">The XmlWriter object to write to</param>
        private static void EnumerateDirectoryStructureXML(string dir, ref XmlWriter writer)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            writer.WriteStartElement("Directory");
            writer.WriteAttributeString("name", dir);
            foreach (DirectoryInfo i in info.EnumerateDirectories())
            {
                EnumerateDirectoryStructureXML(i.FullName, ref writer);
            }
            try
            {
                foreach (string file in Directory.EnumerateFiles(dir))
                {
                    writer.WriteStartElement("File");
                    writer.WriteAttributeString("name", file);
                    writer.WriteEndElement();
                }
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine("Denied Access");
            }
            catch (InvalidOperationException inv)
            {
                Console.WriteLine("Invalid operation: " + inv.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            writer.WriteEndElement();
            writer.Flush();
        }

        public static void ExecuteAnother(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
