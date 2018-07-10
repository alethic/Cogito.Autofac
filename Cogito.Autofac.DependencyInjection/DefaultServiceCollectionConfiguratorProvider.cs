using System;
using System.Collections.Generic;

namespace Cogito.Autofac.DependencyInjection
{

    [RegisterAs(typeof(IServiceCollectionConfiguratorProvider))]
    [RegisterOrder(0)]
    public class DefaultServiceCollectionConfiguratorProvider :
        IServiceCollectionConfiguratorProvider
    {

        readonly IEnumerable<IServiceCollectionConfigurator> configurators;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="configurators"></param>
        public DefaultServiceCollectionConfiguratorProvider(IEnumerable<IServiceCollectionConfigurator> configurators)
        {
            this.configurators = configurators ?? throw new ArgumentNullException(nameof(configurators));
        }

        /// <summary>
        /// Gets the configurations
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IServiceCollectionConfigurator> GetConfigurators()
        {
            return configurators;
        }

    }

}
