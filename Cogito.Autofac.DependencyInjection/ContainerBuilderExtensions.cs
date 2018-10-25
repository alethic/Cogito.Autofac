using System;
using System.Linq;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ContainerBuilderExtensions
    {

        /// <summary>
        /// Populates the <see cref="ContainerBuilder"/> with services registered against the generated <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(
            this ContainerBuilder builder,
            Action<IServiceCollection> configure,
            Func<ServiceDescriptor, bool> filter = null)
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
