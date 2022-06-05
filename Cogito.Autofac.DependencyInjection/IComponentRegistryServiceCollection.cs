using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Specifies the contract for a list of service registrations. Stages registrations so that they can be removed
    /// up until the collection is flushed.
    /// </summary>
    public interface IComponentRegistryServiceCollection : IServiceCollection
    {

        /// <summary>
        /// Flushes all staged registrations to the registry.
        /// </summary>
        void Flush();

    }

}