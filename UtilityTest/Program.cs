using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Reno.Utilities;

namespace UtilityTest
{
    class Program
    {
        static StringBuilder output;
        static void Main(string[] args)
        {
            output = new StringBuilder();
            string command = "d2\" \"c:\\phd2 x86.txt\" \"c:\\ fd\" c:\\users\\kylee";
            string[] tokens = Utility.ParseCommand(command);
            foreach(string token in tokens)
            {
                Console.WriteLine("Token {0}", token);
            }

            Console.Read();
        }

        static void OutputData(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
