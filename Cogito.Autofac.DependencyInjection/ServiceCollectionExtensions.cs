using System;
using System.Linq;

using Autofac;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Configures the service collection in accordance with the components framework.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IServiceCollection Configure(this IServiceCollection services, IComponentContext context)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            foreach (var i in context.Resolve<IOrderedEnumerable<IServiceCollectionConfiguratorProvider>>())
                foreach (var j in i.GetConfigurators())
                    services = j.Apply(services);

            return services;
        }

    }

}
