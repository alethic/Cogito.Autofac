using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Provides component registrations from a set of <see cref="ServiceDescriptor"/> on demand.
    /// </summary>
    class LazyServiceDescriptorRegistrationSource : IRegistrationSource
    {

        readonly Func<IEnumerable<ServiceDescriptor>> getServices;
        List<IComponentRegistration> registrations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="getServices"></param>
        public LazyServiceDescriptorRegistrationSource(Func<IEnumerable<ServiceDescriptor>> getServices)
        {
            this.getServices = getServices ?? throw new ArgumentNullException(nameof(getServices));
        }

        public bool IsAdapterForIndividualComponents => false;

        /// <summary>
        /// Builds the <see cref="IComponentRegistration"/>s from the specified source.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        IEnumerable<IComponentRegistration> CreateRegistrations(IEnumerable<ServiceDescriptor> services)
        {
            foreach (var service in services)
            {
                if (service.ImplementationInstance != null)
                    yield return new ComponentRegistration(
                        Guid.NewGuid(),
                        new ProvidedInstanceActivator(service.ImplementationInstance),
                        service.Lifetime == ServiceLifetime.Singleton ? (IComponentLifetime)new RootScopeLifetime() : new CurrentScopeLifetime(),
                        service.Lifetime == ServiceLifetime.Transient ? InstanceSharing.None : InstanceSharing.Shared,
                        InstanceOwnership.OwnedByLifetimeScope,
                        new[] { new TypedService(service.ServiceType) },
                        new Dictionary<string, object>());

                if (service.ImplementationType != null)
                    yield return new ComponentRegistration(
                        Guid.NewGuid(),
                        new ReflectionActivator(
                            service.ImplementationType,
                            new DefaultConstructorFinder(),
                            new MostParametersConstructorSelector(),
                            Enumerable.Empty<Parameter>(),
                            Enumerable.Empty<Parameter>()),
                        service.Lifetime == ServiceLifetime.Singleton ? (IComponentLifetime)new RootScopeLifetime() : new CurrentScopeLifetime(),
                        service.Lifetime == ServiceLifetime.Transient ? InstanceSharing.None : InstanceSharing.Shared,
                        InstanceOwnership.OwnedByLifetimeScope,
                        new[] { new TypedService(service.ServiceType) },
                        new Dictionary<string, object>());

                if (service.ImplementationFactory != null)
                    yield return new ComponentRegistration(
                        Guid.NewGuid(),
                        new DelegateActivator(
                            service.ServiceType,
                            (c, p) => service.ImplementationFactory(c.Resolve<IServiceProvider>())),
                        service.Lifetime == ServiceLifetime.Singleton ? (IComponentLifetime)new RootScopeLifetime() : new CurrentScopeLifetime(),
                        service.Lifetime == ServiceLifetime.Transient ? InstanceSharing.None : InstanceSharing.Shared,
                        InstanceOwnership.OwnedByLifetimeScope,
                        new[] { new TypedService(service.ServiceType) },
                        new Dictionary<string, object>());
            }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            // initialize registrations on first call
            if (registrations == null)
                if (getServices() is IEnumerable<ServiceDescriptor> source)
                    registrations = CreateRegistrations(source).ToList();

            // return filtered registrations
            if (registrations != null)
                return registrations.Where(i => i.Services.Contains(service));

            // not initialized, no registrations
            return Enumerable.Empty<IComponentRegistration>();
        }

    }

}
