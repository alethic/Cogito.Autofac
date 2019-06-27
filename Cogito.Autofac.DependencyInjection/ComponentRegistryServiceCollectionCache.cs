using System;
using System.Collections.Generic;

using Autofac.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Maintains a cache of registration capabilities added by the <see cref="ComponentRegistryServiceCollection"/>.
    /// </summary>
    class ComponentRegistryServiceCollectionCache
    {

        readonly Dictionary<Guid, ServiceDescriptor[]> components = new Dictionary<Guid, ServiceDescriptor[]>();
        readonly Dictionary<IRegistrationSource, ServiceDescriptor[]> sources = new Dictionary<IRegistrationSource, ServiceDescriptor[]>();

        /// <summary>
        /// Mapping of component ID to resulting <see cref="ServiceDescriptor"/> instances.
        /// </summary>
        public IDictionary<Guid, ServiceDescriptor[]> Components => components;

        /// <summary>
        /// Mapping of resulting <see cref="IRegistrationSource"/>s to <see cref="ServiceDescriptor"/> instances.
        /// </summary>
        public IDictionary<IRegistrationSource, ServiceDescriptor[]> Sources => sources;

    }

}