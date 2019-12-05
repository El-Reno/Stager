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
            string command = @"tree C:\ /F /A";
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = command;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            string output = p.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }

        public static void ExecuteAnother(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
