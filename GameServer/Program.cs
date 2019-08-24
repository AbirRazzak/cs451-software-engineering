using System;
using System.Text;
using Nancy.Hosting.Self;

namespace GameServer
{
    class MainClass
    {
        private static readonly String exitCode = "exit()";

        static void Main(string[] args)
        {
            String consoleInput;
            var uri = new Uri("http://localhost:55555/");
            var config = new HostConfiguration { RewriteLocalhost = true };
            var startupMessage = new StringBuilder("Running on ");
            startupMessage.AppendFormat("{0}. Use {1} to shut down the server.", uri.ToString(), exitCode);
            var host = new NancyHost(config, uri);
            try
            {
                host.Start();
                Console.WriteLine(startupMessage);

                do
                {
                    consoleInput = Console.ReadLine();
                } while (!consoleInput.Equals(exitCode));
                host.Stop();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unknown error has occurred. See below: \n" + ex.ToString());
            }
        }
    }
}
