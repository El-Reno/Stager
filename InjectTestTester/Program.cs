using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reno.Stages;


namespace InjectTestTester
{
    class Program
    {
        static void Main(string[] args)
        {
            //Tester.Execute();
            //Console.WriteLine(DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\NetBeansProjects", "TEXT"));
            Console.WriteLine(DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\Weather Station", "XML"));
            //Console.WriteLine(DirectoryTraversal.EnumerateDirectoryStructure(@"E:\ISOs", "XML"));
            Console.WriteLine(DirectoryTraversal.EnumerateDirectoryStructure(@"C:\Users\kylee\Documents\Weather Station", "TEXT"));
            //Console.WriteLine(DirectoryTraversal.EnumerateDirectoryStructure(@"E:\ISOs", "TEXT"));
            Console.ReadLine();
        }
    }
}
