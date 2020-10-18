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

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ServiceDescriptorExtensions
    {

        static readonly Type OpenGenericRegistrationExtensionsType = typeof(global::Autofac.Module).Assembly.GetType("Autofac.Features.OpenGenerics.OpenGenericRegistrationExtensions");
        static readonly MethodInfo CreateRegistrationBuilderMethod = OpenGenericRegistrationExtensionsType.GetMethod("CreateGenericBuilder", new Type[] { typeof(Type) });
        static readonly Type OpenGenericRegistrationSourceType = typeof(global::Autofac.Module).Assembly.GetType("Autofac.Features.OpenGenerics.OpenGenericRegistrationSource");

        static readonly IEnumerable<Parameter> EmptyParameters = Enumerable.Empty<Parameter>();
        static readonly IDictionary<string, object> EmptyMetadata = new Dictionary<string, object>();

        /// <summary>
        /// Generates a <see cref="IComponentRegistration"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        /// <returns></returns>
        public static IComponentRegistration ToComponentRegistration(this ServiceDescriptor service, object lifetimeScopeTagForSingletons = null)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
                throw new NotSupportedException();

            return new ServiceDescriptorComponentRegistration(
                Guid.NewGuid(),
                GetActivator(service),
                GetComponentLifetime(service, lifetimeScopeTagForSingletons),
                GetInstanceSharing(service),
                InstanceOwnership.OwnedByLifetimeScope,
                new[] { new TypedService(service.ServiceType) },
                EmptyMetadata,
                service);
        }

        /// <summary>
        /// Generates a <see cref="IComponentRegistration"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static IComponentRegistration ToComponentRegistration(this ServiceDescriptor service)
        {
            return ToComponentRegistration(service, null);
        }

        /// <summary>
        /// Generates a <see cref="IRegistrationSource"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="builder"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        /// <returns></returns>
        public static IRegistrationSource ToRegistrationSource(this ServiceDescriptor service, IComponentRegistryBuilder builder, object lifetimeScopeTagForSingletons)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (service.ServiceType.GetTypeInfo().IsGenericTypeDefinition)
            {
                if (service.ImplementationType == null)
                    throw new NotSupportedException("Cannot register open generic types without implementation type.");

                var b = (IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>)CreateRegistrationBuilderMethod.Invoke(null, new object[] { service.ImplementationType });
                var s = (IRegistrationSource)Activator.CreateInstance(
                    OpenGenericRegistrationSourceType,
                    new RegistrationData(new TypedService(service.ServiceType))
                    {
                        Lifetime = GetComponentLifetime(service, lifetimeScopeTagForSingletons),
                        Sharing = GetInstanceSharing(service),
                        Ownership = InstanceOwnership.OwnedByLifetimeScope
                    },
                    b.ResolvePipeline.Clone(),
                    new ReflectionActivatorData(service.ImplementationType));

                return s;
            }

            throw new NotSupportedException();
        }

        /// <summary>
        /// Generates a <see cref="IRegistrationSource"/> that mimics the <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IRegistrationSource ToRegistrationSource(this ServiceDescriptor service, IComponentRegistryBuilder builder)
        {
            return ToRegistrationSource(service, null);
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

        /// <summary>
        /// Gets the component lifetime for a <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="lifetimeScopeTagForSingletons"></param>
        /// <returns></returns>
        static IComponentLifetime GetComponentLifetime(ServiceDescriptor service, object lifetimeScopeTagForSingletons)
        {
            switch (service.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    if (lifetimeScopeTagForSingletons == null)
                        return new RootScopeLifetime();
                    else
                        return new MatchingScopeLifetime(lifetimeScopeTagForSingletons);
                case ServiceLifetime.Transient:
                case ServiceLifetime.Scoped:
                    return new CurrentScopeLifetime();
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Gets the instance sharing mode for a <see cref="ServiceDescriptor"/>.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        static InstanceSharing GetInstanceSharing(ServiceDescriptor service)
        {
            switch (service.Lifetime)
            {
                case ServiceLifetime.Singleton:
                case ServiceLifetime.Scoped:
                    return InstanceSharing.Shared;
                case ServiceLifetime.Transient:
                    return InstanceSharing.None;
                default:
                    throw new InvalidOperationException();
            }
        }

    }

}
