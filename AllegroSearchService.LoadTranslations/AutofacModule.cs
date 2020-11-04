using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllegroSearchService.Bl.ServiceInterfaces;
using AllegroSearchService.Bl.ServiceInterfaces.Repo;
using AllegroSearchService.Bl.Services;
using AllegroSearchService.Bl.Services.Repo;
using Autofac;
using Microsoft.EntityFrameworkCore;

namespace AllegroSearchService.LoadTranslations
{
    public class AutofacModule : Module
    {
        public AutofacModule()
        {

        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<TranslateService>()
               .As<ITranslateService>()
               .InstancePerLifetimeScope();
            builder.RegisterType<TokenService>()
               .As<ITokenService>()
               .InstancePerLifetimeScope();
        }

        public class DataAccessEventsAutofacModule<TContext> : Module where TContext : DbContext
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<ReadOnlyRepository<TContext>>()
                    .As<IReadOnlyRepository>()
                    .InstancePerLifetimeScope();

                builder.RegisterType<WriteOnlyRepository<TContext>>()
                    .As<IWriteOnlyRepository>()
                    .InstancePerLifetimeScope();
            }
        }
    }
}
