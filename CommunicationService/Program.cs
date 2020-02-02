using System;

using System.ServiceProcess;


namespace CommunicationService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new CommunicationService()
            };
            ServiceBase.Run(ServicesToRun);
            Console.Read();
        }
    }
}
