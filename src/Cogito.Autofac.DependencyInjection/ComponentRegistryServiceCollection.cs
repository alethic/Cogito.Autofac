using System;
using System.Collections;
using System.Collections.Generic;

using Autofac.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Provides a <see cref="IServiceCollection"/> implementation which maps operations against a <see cref="IComponentRegistry"/>.
    /// </summary>
    class ComponentRegistryServiceCollection : IServiceCollection
    {

        readonly ComponentRegistryServiceCollectionCache cache;
        readonly object lifetimeScopeTagForSingletons;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        public ComponentRegistryServiceCollection(ComponentRegistryServiceCollectionCache cache, object lifetimeScopeTagForSingletons)
        {
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.lifetimeScopeTagForSingletons = lifetimeScopeTagForSingletons;
        }

        /// <summary>
        /// Flushes all staged registrations to the registry.
        /// </summary>
        public void Flush()
        {
            cache.Flush();
        }

        /// <summary>
        /// Returns whether the registry can be edited.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Gets the <see cref="ServiceDescriptor"/> at the specified position.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ServiceDescriptor this[int index]
        {
            get => cache[index];
            set => cache[index] = value;
        }

        /// <summary>
        /// Returns the count of registrations in the collection.
        /// </summary>
        public int Count => cache.Count;

        /// <summary>
        /// Registers the specified <see cref="ServiceDescriptor"/> against the container.
        /// </summary>
        /// <param name="service"></param>
        public void Add(ServiceDescriptor service)
        {
            cache.Add(service, lifetimeScopeTagForSingletons);
        }

        /// <summary>
        /// Clears the service collection. This is not supported.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        public void Clear()
        {
            throw new NotSupportedException("Cannot clear Autofac ServiceCollection.");
        }

        /// <summary>
        /// Returns <c>true</c> if the service descriptor exists in this service collection.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(ServiceDescriptor item)
        {
            return cache.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            cache.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return cache.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return cache.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotSupportedException("Cannot insert into Autofac ServiceCollection.");
        }

        public bool Remove(ServiceDescriptor item)
        {
            return cache.Remove(item);
        }

        public void RemoveAt(int index)
        {
            cache.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
