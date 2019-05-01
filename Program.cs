using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DotNetEnv;

namespace SeCoucherMoinsBeteRssFeed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Env.Load();
            }
            catch (Exception)
            {
                Console.WriteLine("No .env file found");
            }

            IWebHost host = BuildWebHost(args);
            host.Run();
        }


        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
