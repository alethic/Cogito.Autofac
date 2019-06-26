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
        public static ContainerBuilder Populate(this ContainerBuilder builder, Action<IServiceCollection> configure, Func<ServiceDescriptor, bool> filter = null)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            builder.Populate(Enumerable.Empty<ServiceDescriptor>());
            builder.RegisterCallback(registry => configure(new ComponentRegistryServiceCollection(registry, filter)));
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
