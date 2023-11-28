using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Autofac.Features.Metadata;

using Cogito.Collections;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Underlying cached set of registered service descriptors.
    /// </summary>
    class ComponentRegistryServiceCollectionCache : IDisposable
    {

        readonly IComponentRegistryBuilder builder;
        readonly List<object> registered = new List<object>();
        readonly List<(ServiceDescriptor, object)> staged = new List<(ServiceDescriptor, object)>();
        readonly Dictionary<Guid, ServiceDescriptor[]> components = new Dictionary<Guid, ServiceDescriptor[]>();
        readonly Dictionary<IRegistrationSource, ServiceDescriptor[]> sources = new Dictionary<IRegistrationSource, ServiceDescriptor[]>();
        List<ServiceDescriptor> descriptors = new List<ServiceDescriptor>();
            
        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="builder"></param>
        public ComponentRegistryServiceCollectionCache(IComponentRegistryBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            this.builder.Registered += builder_Registered;
            this.builder.RegistrationSourceAdded += builder_RegistrationSourceAdded;
        }

        /// <summary>
        /// Invoked when a component is registered with the underlying builder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void builder_Registered(object sender, ComponentRegisteredEventArgs args)
        {
            if (args.ComponentRegistryBuilder == builder)
            {
                registered.Add(args.ComponentRegistration);
                descriptors = null;
            }
        }

        /// <summary>
        /// Invoked when a source is registered with the underlying builder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void builder_RegistrationSourceAdded(object sender, RegistrationSourceAddedEventArgs args)
        {
            if (args.ComponentRegistry == builder)
            {
                registered.Add(args.RegistrationSource);
                descriptors = null;
            }
        }

        /// <summary>
        /// Gets the full set of service descriptors that exist in the underlying container builder.
        /// </summary>
        List<ServiceDescriptor> ServiceDescriptors => descriptors ??= registered.SelectMany(i => GetServiceDescriptors(i)).Concat(staged.Select(i => i.Item1)).ToList();

        /// <summary>
        /// Flushes all staged registrations to the registry.
        /// </summary>
        public void Flush()
        {
            // register each of the staged items
            foreach (var (registration, tag) in staged)
                Register(registration, tag);

            // clear the staging area
            staged.Clear();
            descriptors = null;
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
        /// Obtains fake <see cref="ServiceDescriptor"/> entries for the given <see cref="IRegistrationSource"/>.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        IEnumerable<ServiceDescriptor> CreateServiceDescriptors(IRegistrationSource source)
        {
            switch (source)
            {
                case ImplicitRegistrationSource implicitRegistrationSource:
                    yield return ServiceDescriptor.Transient((Type)typeof(ImplicitRegistrationSource).GetField("_type", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(implicitRegistrationSource), svc => throw new NotSupportedException("Registration sources cannot be activated."));
                    break;
                case IRegistrationSource collectionRegistrationSource when collectionRegistrationSource.GetType().FullName == "Autofac.Features.Collections.CollectionRegistrationSource":
                    yield break;
                case IRegistrationSource lazyWithMetadataRegistrationSource when lazyWithMetadataRegistrationSource.GetType().FullName == "Autofac.Features.LazyDependencies.LazyWithMetadataRegistrationSource":
                    yield return ServiceDescriptor.Transient(typeof(Lazy<,>), svc => throw new NotSupportedException("Activators not supported for artificial registration source."));
                    break;
                case IRegistrationSource stronglyTypedMetaRegistrationSource when stronglyTypedMetaRegistrationSource.GetType().FullName == "Autofac.Features.Metadata.StronglyTypedMetaRegistrationSource":
                    yield return ServiceDescriptor.Transient(typeof(Meta<,>), svc => throw new NotSupportedException("Activators not supported for artificial registration source."));
                    break;
                case IRegistrationSource generatedFactoryRegistrationSource when generatedFactoryRegistrationSource.GetType().FullName == "Autofac.Features.GeneratedFactories.GeneratedFactoryRegistrationSource":
                    yield return ServiceDescriptor.Transient(typeof(Func<>), svc => throw new NotSupportedException("Activators not supported for artificial registration source."));
                    break;
                default:
                    yield break;
            }
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
                return ServiceDescriptor.Singleton(service.ServiceType, GenerateImplementationFactory(d, d.LimitType));
            else
                return ServiceDescriptor.Singleton(service.ServiceType, svc => throw new NotSupportedException($"Unknown Activator: {registration.Activator.GetType()}"));
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
                return ServiceDescriptor.Scoped(service.ServiceType, GenerateImplementationFactory(d, d.LimitType));
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
                return ServiceDescriptor.Transient(service.ServiceType, GenerateImplementationFactory(d, d.LimitType));
            else
                return ServiceDescriptor.Transient(service.ServiceType, svc => throw new NotSupportedException($"Unknown Activator: {registration.Activator.GetType()}"));
        }

        /// <summary>
        /// Transforms a <see cref="DelegateActivator"/> into an implementation factory with the appropriate delegate type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="activator"></param>
        /// <returns></returns>
        Func<IServiceProvider, object> GenerateImplementationFactory(DelegateActivator activator, Type returnType)
        {
            // factory function can't actually run, but does have the right type signature
            var func = Expression.Lambda(
                typeof(Func<,>).MakeGenericType(typeof(IServiceProvider), returnType),
                Expression.Block(
                    Expression.Call(typeof(ComponentRegistryServiceCollectionCache), nameof(ThrowNotSupportedException), Array.Empty<Type>(), Expression.Constant("Delegate activators not supported.")),
                    Expression.Convert(Expression.Constant(null), returnType)),
                Expression.Parameter(typeof(IServiceProvider), "provider"));

            return (Func<IServiceProvider, object>)func.Compile();
        }

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/> with the given method.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="NotSupportedException"></exception>
        static void ThrowNotSupportedException(string message)
        {
            throw new NotSupportedException(message);
        }

        /// <summary>
        /// Obtains fake <see cref="ServiceDescriptor"/> entries for the given <see cref="IComponentRegistration"/>.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        IEnumerable<ServiceDescriptor> GetServiceDescriptors(object o)
        {
            if (o == null)
                throw new ArgumentNullException(nameof(o));

            if (o is ServiceDescriptorComponentRegistration serviceDescriptorRegistration)
                yield return serviceDescriptorRegistration.ServiceDescriptor;
            else if (o is IComponentRegistration registration)
                foreach (var descriptor in components.GetOrAdd(registration.Id, _ => CreateServiceDescriptors(registration).ToArray()))
                    yield return descriptor;
            else if (o is ServiceDescriptorRegistrationSource serviceDescriptorRegistrationSource)
                foreach (var descriptor in serviceDescriptorRegistrationSource.ServiceDescriptors)
                    yield return descriptor;
            else if (o is IRegistrationSource source)
                foreach (var descriptor in sources.GetOrAdd(source, _ => CreateServiceDescriptors(source).ToArray()))
                    yield return descriptor;
            else
                yield break;
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
        /// <param name="lifetimeScopeTagForSingletons"></param>
        public void Add(ServiceDescriptor service, object lifetimeScopeTagForSingletons)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            staged.Add((service, lifetimeScopeTagForSingletons));
            descriptors = null;
        }

        public void Clear()
        {
            throw new NotSupportedException("Cannot clear Autofac ServiceCollection.");
        }

        public bool Contains(ServiceDescriptor item)
        {
            return ServiceDescriptors.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            ServiceDescriptors.CopyTo(array, arrayIndex);
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return ServiceDescriptors.GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return ServiceDescriptors.IndexOf(item);
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
                descriptors = null;
                return true;
            }

            // existing descriptor that matches, but outside of our staged items, we cannot support removing
            if (Contains(item))
                throw new NotSupportedException("Cannot remove a service added from a separate call to Populate or which was not registered by Microsoft Dependency Injection.");

            return false;
        }

        /// <summary>
        /// Removes the descriptor at the specified index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        /// <summary>
        /// Disposes of the instance.
        /// </summary>
        public void Dispose()
        {
            // unsubscribe from builder
            if (builder != null)
            {
                builder.Registered -= builder_Registered;
                builder.RegistrationSourceAdded -= builder_RegistrationSourceAdded;
            }
        }

    }

}
