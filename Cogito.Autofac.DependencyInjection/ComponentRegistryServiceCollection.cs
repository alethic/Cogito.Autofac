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
        readonly Func<ServiceDescriptor, bool> filter;
        readonly Dictionary<Guid, ServiceDescriptor[]> components = new Dictionary<Guid, ServiceDescriptor[]>();
        readonly Dictionary<IRegistrationSource, ServiceDescriptor[]> sources = new Dictionary<IRegistrationSource, ServiceDescriptor[]>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="registry"></param>
        /// <param name="filter"></param>
        public ComponentRegistryServiceCollection(IComponentRegistry registry, Func<ServiceDescriptor, bool> filter = null)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
            this.filter = filter;
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
        ServiceDescriptor[] GetServiceDescriptors(IComponentRegistration registration)
        {
            if (registration == null)
                throw new ArgumentNullException(nameof(registration));

            return components.GetOrAdd(registration.Id, _ => CreateServiceDescriptors(registration).ToArray());
        }

        public ServiceDescriptor this[int index]
        {
            get => this.ElementAt(index);
            set => throw new NotSupportedException();
        }

        /// <summary>
        /// Returns the count of registrations in the collection.
        /// </summary>
        public int Count => this.Count();

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

            if (filter?.Invoke(service) != false)
            {
                if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                {
                    var source = service.ToRegistrationSource();
                    sources[source] = new[] { service };
                    registry.AddRegistrationSource(source);
                }
                else
                {
                    var registration = service.ToComponentRegistration();
                    components[registration.Id] = new[] { service };
                    registry.Register(registration);
                }
            }
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
            throw new NotSupportedException();
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return registry.Registrations
                .SelectMany(i => GetServiceDescriptors(i))
                .Concat(sources.SelectMany(i => i.Value))
                .GetEnumerator();
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
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }

}
