using System;
using System.IO;
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

        public static void Enumerate(string dir)
        {
            DirectoryInfo info = new DirectoryInfo(dir);
            foreach(DirectoryInfo i in info.EnumerateDirectories())
            {
                Console.WriteLine(i.FullName);
                try
                {
                    foreach (string file in Directory.EnumerateFiles(i.FullName))
                    {
                        Console.WriteLine(file);
                    }
                }
                catch (UnauthorizedAccessException e) 
                {
                    Console.WriteLine("Denied Access");
                }
            }
        }

        public static void ExecuteAnother(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
