using System;
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

        const string BUILD_CALLBACK_PROPERTY = "Cogito.Autofac.DependencyInjection::BuildCallback";

        /// <summary>
        /// Gets the Microsoft Dependency Injection collection to be registered on container build.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterServiceBuilder(this ContainerBuilder builder)
        {
            var services = (IServiceCollection)builder.Properties.GetOrAdd(typeof(IServiceCollection).FullName, _ => new ServiceCollection());

            // save collection in properties so we can do this multiple times
            if ((bool?)builder.Properties.GetOrDefault(BUILD_CALLBACK_PROPERTY) != true)
                builder.RegisterCallback(cr => cr.Populate(services));

            return services;
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, Action<IServiceCollection> configure, Func<ServiceDescriptor, bool> filter = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            // run services configure into tempporary collection
            var c = new ServiceCollection();
            configure(c);

            // make enumerable, optionally filter
            var l = c.AsEnumerable();
            if (filter != null)
                l = l.Where(filter);

            // populate autofac
            builder.Populate(l);
            return builder;
        }

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(
            this ContainerBuilder builder,
            Action<IComponentContext, IServiceCollection> configure,
            Func<ServiceDescriptor, bool> filter = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            // ensure setup of Autofac service providers
            builder.Populate(Enumerable.Empty<ServiceDescriptor>());

            // close until after build
            IEnumerable<ServiceDescriptor> services = null;

            // after build we will invoke configure method
            builder.RegisterBuildCallback(container =>
            {
                // populate service collection
                var c = new ServiceCollection();
                configure(container, c);

                // make enumerable, optionally filter
                var l = c.AsEnumerable();
                if (filter != null)
                    l = l.Where(filter);

                // set as services collection
                services = l;
            });

            // and provide registrations lazily after build
            builder.RegisterSource(new LazyServiceDescriptorRegistrationSource(() => services));

            return builder;
        }

        /// <summary>
        /// Begins a new lifetime scope after configuring a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="configure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(
            this ILifetimeScope scope,
            Action<IServiceCollection> configure,
            Func<ServiceDescriptor, bool> filter = null)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return new AutofacServiceProvider(scope.BeginLifetimeScope(b => b.Populate(s => { s.Configure(scope); configure(s); }, filter)));
        }

        /// <summary>
        /// Begins a new lifetime scope after configuring a <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="tag"></param>
        /// <param name="configure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static IServiceProvider BeginServiceProviderLifetimeScope(
            this ILifetimeScope scope,
            object tag,
            Action<IServiceCollection> configure,
            Func<ServiceDescriptor, bool> filter = null)
        {
            if (scope == null)
                throw new ArgumentNullException(nameof(scope));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return new AutofacServiceProvider(scope.BeginLifetimeScope(tag, b => b.Populate(s => { s.Configure(scope); configure(s); }, filter)));
        }

    }

}
