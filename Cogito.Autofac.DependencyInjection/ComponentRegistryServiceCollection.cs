using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Core;

using Cogito.Collections;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Provides a <see cref="IServiceCollection"/> implementation which maps operations against a <see cref="IComponentRegistry"/>.
    /// </summary>
    class ComponentRegistryServiceCollection : IServiceCollection
    {

        readonly IComponentRegistry registry;
        readonly ComponentRegistryServiceCollectionCache cache;
        readonly List<IComponentRegistration> registrations = new List<IComponentRegistration>();
        readonly List<IRegistrationSource> sources = new List<IRegistrationSource>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="cache"></param>
        public ComponentRegistryServiceCollection(IComponentRegistry registry, ComponentRegistryServiceCollectionCache cache)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="registry"></param>
        public ComponentRegistryServiceCollection(IComponentRegistry registry) :
            this(registry, null)
        {

        }

        /// <summary>
        /// Flushes all staged registrations to the registry.
        /// </summary>
        public void Flush()
        {
            foreach (var registration in registrations.AsEnumerable().Reverse())
                registry.Register(registration, false);
            registrations.Clear();

            foreach (var source in sources.AsEnumerable().Reverse())
                registry.AddRegistrationSource(source);
            sources.Clear();
        }

        /// <summary>
        /// Obtains fake <see cref="ServiceDescriptor"/> entries for the given <see cref="IComponentRegistration"/>.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        IEnumerable<ServiceDescriptor> CreateServiceDescriptors(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            if (registration.Sharing == InstanceSharing.Shared)
            {
                if (registration.Lifetime is global::Autofac.Core.Lifetime.RootScopeLifetime)
                    foreach (var service in registration.Services.OfType<TypedService>())
                        yield return ServiceDescriptor.Singleton(service.ServiceType, _ => throw new NotSupportedException());

                if (registration.Lifetime is global::Autofac.Core.Lifetime.CurrentScopeLifetime)
                    foreach (var service in registration.Services.OfType<TypedService>())
                        yield return ServiceDescriptor.Scoped(service.ServiceType, _ => throw new NotSupportedException());

                if (registration.Lifetime is global::Autofac.Core.Lifetime.MatchingScopeLifetime)
                    foreach (var service in registration.Services.OfType<TypedService>())
                        yield return ServiceDescriptor.Scoped(service.ServiceType, _ => throw new NotSupportedException());
            }

            if (registration.Sharing == InstanceSharing.None)
            {
                foreach (var service in registration.Services.OfType<TypedService>())
                    yield return ServiceDescriptor.Transient(service.ServiceType, _ => throw new NotSupportedException());
            }
        }

        /// <summary>
        /// Obtains fake <see cref="ServiceDescriptor"/> entries for the given <see cref="IComponentRegistration"/>.
        /// </summary>
        /// <param name="registration"></param>
        /// <returns></returns>
        IEnumerable<ServiceDescriptor> GetServiceDescriptors(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            if (registration is ServiceDescriptorComponentRegistration serviceDescriptorRegistration)
                yield return serviceDescriptorRegistration.ServiceDescriptor;
            else
                foreach (var descriptor in cache.Components.GetOrAdd(registration.Id, _ => CreateServiceDescriptors(registration).ToArray()))
                    yield return descriptor;
        }

        /// <summary>
        /// Obtains fake <see cref="ServiceDescriptor"/> entries for the given <see cref="IRegistrationSource"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        IEnumerable<ServiceDescriptor> GetServiceDescriptors(IRegistrationSource source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (source is ServiceDescriptorRegistrationSource serviceDescriptorSource)
                yield return serviceDescriptorSource.ServiceDescriptors;
        }

        public ServiceDescriptor this[int index]
        {
            get => ServiceDescriptors.ElementAt(index);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the count of registrations in the collection.
        /// </summary>
        public int Count => ServiceDescriptors.Count();

        /// <summary>
        /// Returns whether the registry can be edited.
        /// </summary>
        public bool IsReadOnly => !registry.HasLocalComponents;

        /// <summary>
        /// Registers the specified <see cref="ServiceDescriptor"/> against the container.
        /// </summary>
        /// <param name="service"></param>
        public void Add(ServiceDescriptor service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition == false)
                registrations.Add(service.ToComponentRegistration());
            else
                sources.Add(service.ToRegistrationSource());
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return registry.IsRegistered(new TypedService(item.ServiceType));
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            var l = ServiceDescriptors.ToList();
            for (var i = 0; i < l.Count; i++)
                array[arrayIndex + i] = l[i];
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return ServiceDescriptors.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return this
                .Select((a, i) => new { ServiceDescriptor = a, Index = i })
                .FirstOrDefault(x => Equals(x.ServiceDescriptor, item))?.Index ?? -1;
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotSupportedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            // we can support removing descriptors that are staged
            if (registrations.OfType<ServiceDescriptorComponentRegistration>().FirstOrDefault(i => i.ServiceDescriptor == item) is IComponentRegistration registration)
            {
                registrations.Remove(registration);
                return true;
            }

            // existing descriptor that matches, but ourside of our staged items, we cannot support removing
            if (ServiceDescriptors.Contains(item))
                throw new NotSupportedException("Cannot remove a service added from a separate call to Populate.");

            return false;
        }

        public void RemoveAt(int index)
        {
            Remove(ServiceDescriptors.ElementAt(index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerable<ServiceDescriptor> ServiceDescriptors =>
            registry.Registrations
                .SelectMany(i => GetServiceDescriptors(i))
                .Concat(registry.Sources.SelectMany(i => GetServiceDescriptors(i)))
                .Concat(registrations.SelectMany(i => GetServiceDescriptors(i)))
                .Concat(sources.SelectMany(i => GetServiceDescriptors(i)));

    }

}
