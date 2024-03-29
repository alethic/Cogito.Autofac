﻿using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Cogito.Collections;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ContainerBuilderExtensions
    {

        const string COMPONENT_REGISTRY_SERVICE_CACHE_KEY = "Cogito.Autofac.DependencyInjection::Cache";

        /// <summary>
        /// Creates a new cache for the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        static ComponentRegistryServiceCollectionCache CreateCache(ContainerBuilder builder)
        {
            AutofacRegistration.Populate(builder, Enumerable.Empty<ServiceDescriptor>());
            var cache = new ComponentRegistryServiceCollectionCache(builder.ComponentRegistryBuilder);
            builder.RegisterBuildCallback(scope => DeleteCache(builder, cache));
            return cache;
        }

        /// <summary>
        /// Removes the cache's association with the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="cache"></param>
        static void DeleteCache(ContainerBuilder builder, ComponentRegistryServiceCollectionCache cache)
        {
            cache.Dispose();
            builder.Properties.Remove(COMPONENT_REGISTRY_SERVICE_CACHE_KEY);
        }

        /// <summary>
        /// Gets the cache registered with the builder.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        static ComponentRegistryServiceCollectionCache GetOrCreateCache(ContainerBuilder builder)
        {
            return (ComponentRegistryServiceCollectionCache)builder.Properties.GetOrAdd(COMPONENT_REGISTRY_SERVICE_CACHE_KEY, _ => CreateCache(builder));
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, Action<IServiceCollection> configure, object lifetimeScopeTagForSingletons = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var cache = GetOrCreateCache(builder);
            builder.RegisterCallback(b => { var c = new ComponentRegistryServiceCollection(cache, lifetimeScopeTagForSingletons); configure(c); c.Flush(); });
            return builder;
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, Action<IServiceCollection> configure)
        {
            return Populate(builder, configure, null);
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, IEnumerable<ServiceDescriptor> services, object lifetimeScopeTagForSingletons = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var cache = GetOrCreateCache(builder);
            builder.RegisterCallback(b => { var c = new ComponentRegistryServiceCollection(cache, lifetimeScopeTagForSingletons); c.AddRange(services); c.Flush(); });
            return builder;
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, IEnumerable<ServiceDescriptor> services)
        {
            return Populate(builder, services, null);
        }

        /// <summary>
        /// Begins a new lifetime scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(this ILifetimeScope scope)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new AutofacServiceProvider(scope.BeginLifetimeScope());
        }

        /// <summary>
        /// Begins a new lifetime scope after configuring a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(this ILifetimeScope scope, Action<IServiceCollection> configure)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return new AutofacServiceProvider(scope.BeginLifetimeScope(builder => Populate(builder, configure)));
        }

        /// <summary>
        /// Begins a new lifetime scope.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(this ILifetimeScope scope, object tag)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));

            return new AutofacServiceProvider(scope.BeginLifetimeScope(tag));
        }

        /// <summary>
        /// Begins a new lifetime scope after configuring a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="tag"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(this ILifetimeScope scope, object tag, Action<IServiceCollection> configure)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return new AutofacServiceProvider(scope.BeginLifetimeScope(tag, builder => Populate(builder, configure)));
        }

    }

}
