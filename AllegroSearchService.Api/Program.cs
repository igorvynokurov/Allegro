using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Autofac;
using Autofac.Extensions.DependencyInjection;
using AllegroSearchService.Api.Automapper;
using System.IO;
using static AllegroSearchService.Api.Automapper.AutofacModule;
using AllegroSearchService.Data.Config;
using Serilog;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).UseSerilog()
            .Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((context, containerBuilder) =>
                {
                    containerBuilder.RegisterModule<AutofacModule>();
                    //containerBuilder.RegisterModule(new IntegrationEventsAutofacModule<MessageBusListener>(context.Configuration));
                    containerBuilder.RegisterModule(new DataAccessEventsAutofacModule<SSDbContext>());
                })
                .ConfigureWebHostDefaults(webBuilder =>
                { 
                    webBuilder.UseStartup<Startup>();
                });
    }
}
