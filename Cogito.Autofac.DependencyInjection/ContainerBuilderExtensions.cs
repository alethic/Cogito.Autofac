using System;

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
        /// <param name="services"></param>
        /// <returns></returns>
        public static ContainerBuilder Populate(this ContainerBuilder builder, Action<IServiceCollection> services)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var c = new ServiceCollection();
            services(c);
            builder.Populate(c);
            return builder;
        }

    }

}
