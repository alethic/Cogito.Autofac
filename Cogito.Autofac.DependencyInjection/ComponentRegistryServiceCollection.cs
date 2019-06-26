using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Core;
using Cogito.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public class ComponentRegistryServiceCollection : IServiceCollection
    {

        readonly IComponentRegistry registry;
        readonly Dictionary<Guid, ServiceDescriptor[]> descriptorCache = new Dictionary<Guid, ServiceDescriptor[]>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="registry"></param>
        public ComponentRegistryServiceCollection(IComponentRegistry registry)
        {
            this.registry = registry ?? throw new ArgumentNullException(nameof(registry));
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

            return descriptorCache.GetOrAdd(registration.Id, _ => CreateServiceDescriptors(registration).ToArray());
        }

        IComponentRegistration GetComponentRegistration(ServiceDescriptor service)
        {

        }

        public ServiceDescriptor this[int index]
        {
            get => registry.Registrations.SelectMany(i => GetServiceDescriptors(i)).ElementAt(index);
            set => throw new NotSupportedException();
        }

        public int Count => registry.Registrations.SelectMany(i => GetServiceDescriptors(i)).Count();

        public bool IsReadOnly => !registry.HasLocalComponents;

        public void Add(ServiceDescriptor item)
        {
            throw new NotImplementedException();
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
            return registry.Registrations.Select(i => ToServiceDescriptor(i));
        }

        ServiceDescriptor ToServiceDescriptor(IComponentRegistration registration)
        {
            switch (registration.Sharing)
            {
                case InstanceSharing.Shared:
            }
        }

        public int IndexOf(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(ServiceDescriptor item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

}
