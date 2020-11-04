using AllegroSearchService.Bl.ServiceInterfaces;
using AllegroSearchService.Data.Config;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AllegroSearchService.LoadTranslations.AutofacModule;

namespace AllegroSearchService.LoadTranslations
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = BuildContainer();
            var translateService = container.Resolve<ITranslateService>();


            var res = GetTranslationsFromFolder("c:\\Translations");
            var items = res.Where(x => x.Key.Length <= 250).Select(x=>x.Key).ToList();
            var guid = Guid.NewGuid();
            foreach(var it in res)
            {
                translateService.AddRecord(it.Key.ToLower(), it.Value, "pl", "ru", guid).Wait();
            }

            System.Console.ReadLine();
        }

        private static IDictionary<string,string> GetTranslationsFromFolder(string folder)
        {
            var files = Directory.GetFiles(folder);
            string line;
            var res = new Dictionary<string, string>();

            foreach (var f in files)
            {
                var file =
                        new System.IO.StreamReader(f);
                while ((line = file.ReadLine()) != null)
                {
                    var l = line.Trim();
                    if (!String.IsNullOrEmpty(l))
                    {
                        var arr = l.Split('/');
                        if (arr.Length == 4)
                        {
                            arr = new string[] { arr[0] + "/" + arr[1], arr[2] + "/" + arr[3] };
                            System.Console.WriteLine(arr[0] + arr[1]);
                        }

                        if (arr.Length != 2)
                        {
                            throw new ApplicationException("Error line " + line);
                        }

                        if (!res.ContainsKey(arr[0]))
                        {
                            res.Add(arr[0].Trim(), arr[1].Trim());
                            //System.Console.WriteLine(arr[0] + arr[1]);
                        }
                    }
                }

                file.Close();
                //System.Console.WriteLine("There were {0} lines.", counter);
                // Suspend the screen.  
                
            }
            return res;
        }

        static IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<AutofacModule>();
            //containerBuilder.RegisterModule(new IntegrationEventsAutofacModule<MessageBusListener>(context.Configuration));
            containerBuilder.RegisterModule(new DataAccessEventsAutofacModule<SSDbContext>());
            containerBuilder
                .Register(c =>
                {
                    //var config = c.ResolveOptional<IConfiguration>();

                    var opt = new DbContextOptionsBuilder<SSDbContext>();
                    opt.UseSqlServer("Data Source=allegrosearchservice.database.windows.net;Initial Catalog=AlegroSearchService;User ID=Igor;Password=ZGIA_01078445iv;Connect Timeout=60;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

                    return new SSDbContext(opt.Options);
                }).InstancePerLifetimeScope();
            return containerBuilder.Build();
        }
    }
}
