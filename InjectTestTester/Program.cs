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
            //Console.WriteLine(Tester.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\NetBeansProjects", "TEXT"));
            Console.WriteLine(Tester.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\Weather Station", "XML"));
            //Console.WriteLine(Tester.EnumerateDirectoryStructure(@"E:\ISOs", "XML"));
            Console.WriteLine(Tester.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\Weather Station", "TEXT"));
            Console.WriteLine(Tester.EnumerateDirectoryStructure(@"E:\ISOs", "TEXT"));
            Console.ReadLine();
        }
    }
}
