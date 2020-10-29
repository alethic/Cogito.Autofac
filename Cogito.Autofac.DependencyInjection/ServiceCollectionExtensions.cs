using System;
using System.Linq;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Registers the Autofac container with the <see cref="IServiceCollection"/>. Not required for ASP.Net Core 3 nor the generic host.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutofac(this IServiceCollection services, Action<ContainerBuilder> configure = null)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            return AddAutofac(services, () => new AutofacServiceProviderFactory(configure));
        }

        /// <summary>
        /// Adds an Autofac <see cref="IServiceProviderFactory{TContainerBuilder}"/> implementation to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="scope"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddAutofac(this IServiceCollection services, ILifetimeScope scope, Action<ContainerBuilder> configure = null)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (scope is null)
                throw new ArgumentNullException(nameof(scope));

            return AddAutofac(services, () => new AutofacChildLifetimeScopeServiceProviderFactory(scope, configure));
        }

        /// <summary>
        /// Adds an Autofac <see cref="IServiceProviderFactory{TContainerBuilder}"/> implementation to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        static IServiceCollection AddAutofac<TContainerBuilder>(this IServiceCollection services, Func<IServiceProviderFactory<TContainerBuilder>> func)
        {
            var d = services.FirstOrDefault(i => i.ServiceType == typeof(IServiceProviderFactory<IServiceCollection>));
            if (d != null)
                services.Remove(d);

            services.AddSingleton(ctx => func());
            services.AddSingleton(CreateHostingServiceProviderFactory<TContainerBuilder>);
            return services;
        }

        /// <summary>
        /// Creates a <see cref="IServiceProviderFactory{IServiceCollection}"/> that can function as the hosting container.
        /// </summary>
        /// <typeparam name="TContainerBuilder"></typeparam>
        /// <param name="provider"></param>
        /// <returns></returns>
        static IServiceProviderFactory<IServiceCollection> CreateHostingServiceProviderFactory<TContainerBuilder>(IServiceProvider provider)
        {
            var builder = provider.GetRequiredService<IServiceProviderFactory<TContainerBuilder>>();
            return new AutofacHostingServiceProviderFactory(services => builder.CreateServiceProvider(builder.CreateBuilder(services)));
        }

        /// <summary>
        /// Provides an implementation of <see cref="IServiceProviderFactory{IServiceCollection}"/>.
        /// </summary>
        class AutofacHostingServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
        {

            readonly Func<IServiceCollection, IServiceProvider> factory;

            /// <summary>
            /// Initializes a new instances.
            /// </summary>
            /// <param name="factory"></param>
            public AutofacHostingServiceProviderFactory(Func<IServiceCollection, IServiceProvider> factory)
            {
                this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
            }

            public IServiceCollection CreateBuilder(IServiceCollection services)
            {
                return services;
            }

            public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
            {
                return factory(containerBuilder);
            }

        }

    }

}
