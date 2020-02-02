using System;
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
            stager = new StageZero(5, 5);
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
