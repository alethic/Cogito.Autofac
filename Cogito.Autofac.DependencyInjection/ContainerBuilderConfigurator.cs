using System;
using System.Collections.Generic;

using Autofac;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Provides a utility for appending configurators to generate a lifetime scope.
    /// </summary>
    public class ContainerBuilderConfigurator
    {

        readonly object tag;
        readonly List<Action<ContainerBuilder>> configurators = new List<Action<ContainerBuilder>>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="tag"></param>
        public ContainerBuilderConfigurator(object tag = null)
        {
            this.tag = tag;
        }

        /// <summary>
        /// Adds a new configurator to the builder.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public ContainerBuilderConfigurator Configure(Action<ContainerBuilder> configure)
        {
            if (configure != null)
                configurators.Add(configure);

            return this;
        }

        /// <summary>
        /// Builds a new scope from the registered configuration.
        /// </summary>
        /// <returns></returns>
        public ILifetimeScope BeginLifetimeScope(ILifetimeScope parent)
        {
            if (tag != null)
                return parent.BeginLifetimeScope(tag, Configure);
            else
                return parent.BeginLifetimeScope(Configure);
        }

        void Configure(ContainerBuilder builder)
        {
            // execute remaining configurators
            foreach (var configurator in configurators)
                configurator?.Invoke(builder);
        }

    }

}