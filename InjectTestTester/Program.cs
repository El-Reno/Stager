using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InjectTest;

namespace InjectTestTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Tester.Execute();
            Console.WriteLine(Tester.EnumerateDirectoryStructure(@"C:\Windows\security"));
            Console.ReadLine();
        }
    }
}
