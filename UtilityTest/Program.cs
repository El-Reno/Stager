using System;
using Reno.Utilities;

namespace UtilityTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string command = "d2\" \"c:\\phd2 x86.txt\" \"c:\\ fd\" c:\\users\\kylee";
            string[] tokens = Utility.ParseCommand(command);
            foreach(string token in tokens)
            {
                Console.WriteLine("Token {0}", token);
            }
             
        }
    }
}
