using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Roslyn.Hosts.WebApp
{
    public class Program
    {
        public bool Property { get; private set; } = false;

        public static void Main(string[] args)
        {
            var date = DateTime.Now;

            bool isA = true;
            bool isD,isC = false;


            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
