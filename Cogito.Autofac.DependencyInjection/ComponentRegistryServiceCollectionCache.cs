using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;

using Cogito.Collections;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Underlying cached set of registered service descriptors.
    /// </summary>
    class ComponentRegistryServiceCollectionCache
    {

        readonly IComponentRegistryBuilder builder;
        readonly List<IComponentRegistration> registered = new List<IComponentRegistration>();
        readonly List<(ServiceDescriptor, object)> staged = new List<(ServiceDescriptor, object)>();
        readonly Dictionary<Guid, ServiceDescriptor[]> components = new Dictionary<Guid, ServiceDescriptor[]>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="cache"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        public ComponentRegistryServiceCollectionCache(IComponentRegistryBuilder builder)
        {
            this.builder = builder;
            this.builder.Registered += builder_Registered;
        }

        /// <summary>
        /// Mapping of component ID to resulting <see cref="ServiceDescriptor"/> instances.
        /// </summary>
        public IDictionary<Guid, ServiceDescriptor[]> Components => components;

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
        /// Flushes all staged registrations to the registry.
        /// </summary>
        public void Flush()
        {
            // register each of the staged items
            foreach (var (registration, tag) in staged.AsEnumerable().Reverse())
                Register(registration, tag);

            // clear the staging area
            staged.Clear();
        }

        /// <summary>
        /// Registers the specified service descriptor.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        void Register(ServiceDescriptor registration, object lifetimeScopeTagForSingletons)
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
                        yield return ToSingletonServiceDescriptor(registration, service);

                if (registration.Lifetime is global::Autofac.Core.Lifetime.CurrentScopeLifetime)
                    foreach (var service in registration.Services.OfType<TypedService>())
                        yield return ToScopedServiceDescriptor(registration, service);

                if (registration.Lifetime is global::Autofac.Core.Lifetime.MatchingScopeLifetime)
                    foreach (var service in registration.Services.OfType<TypedService>())
                        yield return ToScopedServiceDescriptor(registration, service);
            }

            if (registration.Sharing == InstanceSharing.None)
            {
                foreach (var service in registration.Services.OfType<TypedService>())
                    yield return ToTransientServiceDescriptor(registration, service);
            }
        }

        /// <summary>
        /// Creates a singleton service descsriptor from the specified registration for the specified service.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        ServiceDescriptor ToSingletonServiceDescriptor(IComponentRegistration registration, TypedService service)
        {
            if (registration.Activator is ReflectionActivator r)
                return ServiceDescriptor.Singleton(service.ServiceType, r.LimitType);
            else if (registration.Activator is ProvidedInstanceActivator i)
                return ServiceDescriptor.Singleton(service.ServiceType, typeof(ProvidedInstanceActivator).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(i));
            else if (registration.Activator is DelegateActivator d)
                return ServiceDescriptor.Singleton(service.ServiceType, svc => throw new NotSupportedException("Delegate activators not supported for singleton services."));
            else
                return ServiceDescriptor.Singleton(service.ServiceType, _ => throw new NotSupportedException($"Unknown Activator: {registration.Activator.GetType()}"));
        }

        /// <summary>
        /// Creates a singleton service descsriptor from the specified registration for the specified service.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        ServiceDescriptor ToScopedServiceDescriptor(IComponentRegistration registration, TypedService service)
        {
            if (registration.Activator is ReflectionActivator r)
                return ServiceDescriptor.Scoped(service.ServiceType, r.LimitType);
            else if (registration.Activator is DelegateActivator d)
                return ServiceDescriptor.Scoped(service.ServiceType, svc => throw new NotSupportedException("Delegate activators not supported for Scoped services."));
            else
                return ServiceDescriptor.Scoped(service.ServiceType, _ => throw new NotSupportedException($"Unknown Activator: {registration.Activator.GetType()}"));
        }

        /// <summary>
        /// Creates a singleton service descsriptor from the specified registration for the specified service.
        /// </summary>
        /// <param name="registration"></param>
        /// <param name="service"></param>
        /// <returns></returns>
        ServiceDescriptor ToTransientServiceDescriptor(IComponentRegistration registration, TypedService service)
        {
            if (registration.Activator is ReflectionActivator r)
                return ServiceDescriptor.Transient(service.ServiceType, r.LimitType);
            else if (registration.Activator is DelegateActivator d)
                return ServiceDescriptor.Transient(service.ServiceType, svc => throw new NotSupportedException("Delegate activators not supported for transient services."));
            else
                return ServiceDescriptor.Transient(service.ServiceType, _ => throw new NotSupportedException($"Unknown Activator: {registration.Activator.GetType()}"));
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
                foreach (var descriptor in Components.GetOrAdd(registration.Id, _ => CreateServiceDescriptors(registration).ToArray()))
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
            set => throw new NotSupportedException("Cannot set indexes on Autofac ServiceCollection.");
        }

        /// <summary>
        /// Returns the count of registrations in the collection.
        /// </summary>
        public int Count => ServiceDescriptors.Count();

        /// <summary>
        /// Registers the specified <see cref="ServiceDescriptor"/> against the container.
        /// </summary>
        /// <param name="service"></param>
        public void Add(ServiceDescriptor service, object lifetimeScopeTagForSingletons)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            staged.Add((service, lifetimeScopeTagForSingletons));
        }

        public void Clear()
        {
            throw new NotSupportedException("Cannot clear Autofac ServiceCollection.");
        }

        public bool Contains(ServiceDescriptor item)
        {
            return builder.IsRegistered(new TypedService(item.ServiceType)) || staged.Any(i => i.Item1 == item);
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
            throw new NotSupportedException("Cannot insert into Autofac ServiceCollection.");
        }

        public bool Remove(ServiceDescriptor item)
        {
            // we can support removing descriptors that are staged
            var remove = staged.FirstOrDefault(i => i.Item1 == item);
            if (remove.Item1 != null)
            {
                staged.Remove(remove);
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

        /// <summary>
        /// Gets the full set of service descriptors that exist in the underlying container builder.
        /// </summary>
        IEnumerable<ServiceDescriptor> ServiceDescriptors => registered
            .SelectMany(i => GetServiceDescriptors(i))
            .Concat(staged.Select(i => i.Item1));

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
