using System;
using System.Linq;

using Autofac;
using Autofac.Builder;
using Autofac.Core.Registration;

using Cogito.Reflection;

namespace Cogito.Autofac
{

    public static class ContainerBuilderExtensions
    {

        /// <summary>
        /// Registers all modules found in all assemblies.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterAllAssemblyModules(this ContainerBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.RegisterAssemblyModules(SafeAssemblyLoader.LoadAll().ToArray());
        }

        /// <summary>
        /// Registers a delegate, accepting another component, as a component.
        /// </summary>
        /// <typeparam name="TWith"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterWith<TWith, T>(this ContainerBuilder builder, Func<TWith, T> func)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            return builder.Register(ctx => func(ctx.Resolve<TWith>()));
        }

    }

}
