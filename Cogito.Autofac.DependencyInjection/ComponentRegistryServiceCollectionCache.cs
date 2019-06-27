using System;
using System.Collections.Generic;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Maintains a cache of registration capabilities added by the <see cref="ComponentRegistryServiceCollection"/>.
    /// </summary>
    class ComponentRegistryServiceCollectionCache
    {

        readonly Dictionary<Guid, ServiceDescriptor[]> components = new Dictionary<Guid, ServiceDescriptor[]>();

        /// <summary>
        /// Mapping of component ID to resulting <see cref="ServiceDescriptor"/> instances.
        /// </summary>
        public IDictionary<Guid, ServiceDescriptor[]> Components => components;

    }

}