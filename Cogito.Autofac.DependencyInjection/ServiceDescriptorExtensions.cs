using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.ProvidedInstance;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using Autofac.Features.OpenGenerics;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ServiceDescriptorExtensions
    {

        static readonly IEnumerable<Parameter> EmptyParameters = Enumerable.Empty<Parameter>();
        static readonly IDictionary<string, object> EmptyMetadata = new Dictionary<string, object>();

        /// <summary>
        /// Generates a <see cref="IComponentRegistration"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IComponentRegistration ToComponentRegistration(this ServiceDescriptor service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                throw new NotSupportedException();

            return new ServiceDescriptorComponentRegistration(
                new ComponentRegistration(
                    Guid.NewGuid(),
                    GetActivator(service),
                    service.Lifetime == ServiceLifetime.Singleton ? (IComponentLifetime)new RootScopeLifetime() : new CurrentScopeLifetime(),
                    service.Lifetime == ServiceLifetime.Transient ? InstanceSharing.None : InstanceSharing.Shared,
                    InstanceOwnership.OwnedByLifetimeScope,
                    new[] { new TypedService(service.ServiceType) },
                    EmptyMetadata),
                service);
        }

        /// <summary>
        /// Generates a <see cref="IRegistrationSource"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IRegistrationSource ToRegistrationSource(this ServiceDescriptor service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                return new ServiceDescriptorRegistrationSource(
                    new OpenGenericRegistrationSource(
                        new RegistrationData(new TypedService(service.ServiceType)),
                        new ReflectionActivatorData(service.ImplementationType)),
                    service);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Gets the activator for the given <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        static IInstanceActivator GetActivator(ServiceDescriptor service)
        {
            if (service.ImplementationInstance != null)
                return new ProvidedInstanceActivator(
                    service.ImplementationInstance);

            if (service.ImplementationType != null)
                return new ReflectionActivator(
                    service.ImplementationType,
                    new DefaultConstructorFinder(),
                    new MostParametersConstructorSelector(),
                    EmptyParameters,
                    EmptyParameters);

            if (service.ImplementationFactory != null)
                return new DelegateActivator(
                    service.ServiceType,
                    (c, p) => service.ImplementationFactory(c.Resolve<IServiceProvider>()));

            throw new InvalidOperationException();
        }

    }

}
