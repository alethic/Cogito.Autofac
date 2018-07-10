using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Applies configuration to a services collection.
    /// </summary>
    public interface IServiceCollectionConfigurator
    {

        /// <summary>
        /// Applies configuration to the services collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        IServiceCollection Apply(IServiceCollection services);

    }

}
