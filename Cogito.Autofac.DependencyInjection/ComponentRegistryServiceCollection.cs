using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Features.OpenGenerics;

using Cogito.Collections;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Provides a <see cref="IServiceCollection"/> implementation which maps operations against a <see cref="IComponentRegistry"/>.
    /// </summary>
    class ComponentRegistryServiceCollection : IServiceCollection, IDisposable
    {

        readonly IComponentRegistryBuilder builder;
        readonly ComponentRegistryServiceCollectionCache cache;
        readonly List<IComponentRegistration> registered = new List<IComponentRegistration>();
        readonly List<ServiceDescriptor> staged = new List<ServiceDescriptor>();
        readonly object lifetimeScopeTagForSingletons;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="cache"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        public ComponentRegistryServiceCollection(IComponentRegistryBuilder builder, ComponentRegistryServiceCollectionCache cache, object lifetimeScopeTagForSingletons)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this.lifetimeScopeTagForSingletons = lifetimeScopeTagForSingletons;

            builder.Registered += builder_Registered;
        }

        /// <summary>
        /// Invoked when a component is registered with the underlying builder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void builder_Registered(object sender, ComponentRegisteredEventArgs args)
        {
            registered.Add(args.ComponentRegistration);
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder"></param>
        public ComponentRegistryServiceCollection(IComponentRegistryBuilder builder) :
            this(builder, null, null)
        {

        }

        /// <summary>
        /// Flushes all staged registrations to the registry.
        /// </summary>
        public void Flush()
        {
            // register each of the staged items
            foreach (var registration in staged.AsEnumerable().Reverse())
                Register(registration);

            // clear the staging area
            staged.Clear();
        }

        /// <summary>
        /// Registers the specified service descriptor.
        /// </summary>
        /// <param name="registration"></param>
        void Register(ServiceDescriptor registration)
        {
            if (registration.ServiceType.GetTypeInfo().IsGenericTypeDefinition == false)
                builder.Register(registration.ToComponentRegistration(lifetimeScopeTagForSingletons));
            else
                builder.AddRegistrationSource(registration.ToRegistrationSource(builder, lifetimeScopeTagForSingletons));
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

        /// <summary>
        /// Gets the <see cref="ServiceDescriptor"/> at the specified position.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
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
        public bool IsReadOnly => false;

        /// <summary>
        /// Registers the specified <see cref="ServiceDescriptor"/> against the container.
        /// </summary>
        /// <param name="service"></param>
        public void Add(ServiceDescriptor service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            staged.Add(service);
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return staged.Contains(item) || builder.IsRegistered(new TypedService(item.ServiceType));
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
            return ServiceDescriptors
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
            if (staged.Contains(item))
            {
                staged.Remove(item);
                return true;
            }

            // existing descriptor that matches, but outside of our staged items, we cannot support removing
            if (ServiceDescriptors.Contains(item))
                throw new NotSupportedException("Cannot remove a service added from a separate call to Populate or which was not registered by Microsoft Dependency Injection.");

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
            registered
                .SelectMany(i => GetServiceDescriptors(i))
                .Concat(staged);

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            // unsubscribe from builder
            if (builder != null)
                builder.Registered -= builder_Registered;
        }

    }

}
