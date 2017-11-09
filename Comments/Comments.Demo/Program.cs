using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

namespace Comments.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory());

            int? port = GetPort();

            if (port.HasValue)
                builder.UseUrls("http://localhost:" + port);
            else
                builder.UseIISIntegration();

            builder
                .UseStartup<Startup>();

            var host = builder.Build();

            host.Run();
        }

        private static int? GetPort()
        {
            if (File.Exists("port"))
            {
                string port = File.ReadAllText("port");
                if (int.TryParse(port, out int parsedPort))
                {
                    return parsedPort;
                }
            }
            return null;
        }
    }
}
