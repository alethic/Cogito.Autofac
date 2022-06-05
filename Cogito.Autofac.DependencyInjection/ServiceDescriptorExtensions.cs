﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                throw new NotSupportedException("Cannot convert generic type definition to component registration.");

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
                    throw new NotSupportedException("No implementation type.");

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

                // wrap in custom registration source to keep track of descriptor
                return new ServiceDescriptorRegistrationSource(s, new[] { service });
            }

            throw new NotSupportedException("Cannot convert non-generic ServiceDescriptor to RegistrationSource.");
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
            {
                // generate a factory method that has the correct return type
                // TryAddEnumerable actually probs the return type of the function at runtime to determine equality
                // so our method needs to be translatable back to that format
                var typeArguments = service.ImplementationFactory.GetType().GenericTypeArguments;
                var implementationType = typeArguments[1];
                var componentContextParameter = Expression.Parameter(typeof(IComponentContext), "context");
                var parameterParameter = Expression.Parameter(typeof(IEnumerable<Parameter>), "parameters");
                var resolveMethodInfo = typeof(ResolutionExtensions).GetMethods().First(i => i.Name == "Resolve" && i.IsGenericMethodDefinition && i.GetGenericArguments().Length == 1);
                var func = Expression.Lambda(
                    typeof(Func<,,>).MakeGenericType(typeof(IComponentContext), typeof(IEnumerable<Parameter>), implementationType),
                    Expression.Invoke(
                        Expression.Constant(service.ImplementationFactory),
                        Expression.Call(resolveMethodInfo.MakeGenericMethod(typeof(IServiceProvider)), componentContextParameter)),
                    componentContextParameter,
                    parameterParameter);

                // generate activator
                return new DelegateActivator(service.ServiceType, (Func<IComponentContext, IEnumerable<Parameter>, object>)func.Compile());
            }

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
