using System.Collections.Generic;

namespace Cogito.Autofac.DependencyInjection
{

    public interface IServiceCollectionConfiguratorProvider
    {

        IEnumerable<IServiceCollectionConfigurator> GetConfigurators();

    }

}