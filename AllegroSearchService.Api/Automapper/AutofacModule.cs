using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;

namespace AllegroSearchService.Api.Automapper
{
    public class AutofacModule : Module
    {
        public AutofacModule()
        {

        }

        protected override void Load(ContainerBuilder builder)
        {
            /*builder.RegisterType<OrdersService>()
               .As<IOrdersService>()
               .InstancePerLifetimeScope();*/
        }
    }
}
