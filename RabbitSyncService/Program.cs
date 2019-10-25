#define HandleStopStart
//#define ServiceOnly

using System.Diagnostics;
using System.IO;
using System.Linq;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace RabbitSyncService
{
    public class Program
    {
#if ServiceOnly
        #region ServiceOnly
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().RunAsService();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Configure the app here.
                })
                .UseUrls($"http://+:5010")
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>();
        }
        #endregion
#endif

#if HandleStopStart
        #region HandleStopStart
        public static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var builder = CreateWebHostBuilder(args.Where(arg => arg != "--console").ToArray(), isService ? 5010 : 5000);

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                builder.UseContentRoot(pathToContentRoot);
            }

            var host = builder.Build();

            if (isService)
            {
                host.RunAsCustomService();
            }
            else
            {
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, int port) =>
             new WebHostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Configure the app here.
                })
                 .UseKestrel()
                 .ConfigureServices(services => services.AddAutofac())
                 .UseUrls($"http://+:{port}")
                .UseStartup<Startup>();
        #endregion
#endif
    }
}
