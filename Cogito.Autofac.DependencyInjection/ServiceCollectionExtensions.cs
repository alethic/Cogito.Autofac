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
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IServiceCollection Configure(this IServiceCollection services, ILifetimeScope scope)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            foreach (var i in scope.Resolve<IOrderedEnumerable<IServiceCollectionConfiguratorProvider>>())
                foreach (var j in i.GetConfigurators())
                    services = j.Apply(services);

            return services;
        }

    }

}
