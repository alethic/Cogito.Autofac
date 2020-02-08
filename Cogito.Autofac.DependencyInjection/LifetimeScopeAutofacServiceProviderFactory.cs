using System;

using Autofac;
using Autofac.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// A factory for creating a <see cref="ContainerBuilder"/> and an <see cref="IServiceProvider" />.
    /// </summary>
    public class LifetimeScopeAutofacServiceProviderFactory : IServiceProviderFactory<ContainerBuilderConfigurator>
    {

        readonly ILifetimeScope parent;
        readonly object tag;
        readonly Action<ContainerBuilder> configure;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        /// <param name="configure"></param>
        public LifetimeScopeAutofacServiceProviderFactory(ILifetimeScope parent, object tag = null, Action<ContainerBuilder> configure = null)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.tag = tag;
            this.configure = configure;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configure"></param>
        public LifetimeScopeAutofacServiceProviderFactory(ILifetimeScope parent, Action<ContainerBuilder> configure = null) :
            this(parent, null, configure)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        public LifetimeScopeAutofacServiceProviderFactory(ILifetimeScope parent, object tag = null) :
            this(parent, tag, null)
        {

        }

        /// <summary>
        /// Creates a container builder from an <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <returns>A container builder that can be used to create an <see cref="IServiceProvider" />.</returns>
        public ContainerBuilderConfigurator CreateBuilder(IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var c = new ContainerBuilderConfigurator(tag);
            c.Configure(configure);
            c.Configure(b => b.Populate(services));
            return c;
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider" /> from the container builder.
        /// </summary>
        /// <param name="builder">The container builder.</param>
        /// <returns>An <see cref="IServiceProvider" />.</returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilderConfigurator builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return new AutofacServiceProvider(builder.BeginLifetimeScope(parent));
        }

    }

}
