using System;
using System.Linq;

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Registration;

using Cogito.Reflection;

using Microsoft.Extensions.DependencyModel;

namespace Cogito.Autofac
{

    public static class ContainerBuilderExtensions
    {

#if NETSTANDARD2_0_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER

        /// <summary>
        /// Registers all modules found in all assemblies.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dependencyContext"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterAllAssemblyModules(this ContainerBuilder builder, DependencyContext dependencyContext)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (dependencyContext is null)
                throw new ArgumentNullException(nameof(dependencyContext));

            return builder.RegisterAssemblyModules(SafeAssemblyLoader.LoadAll(dependencyContext).ToArray());
        }

        /// <summary>
        /// Registers all modules found in all assemblies.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dependencyContext"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterAllAssemblyModules<TModule>(this ContainerBuilder builder, DependencyContext dependencyContext)
            where TModule : IModule
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));
            if (dependencyContext is null)
                throw new ArgumentNullException(nameof(dependencyContext));

            return builder.RegisterAssemblyModules<TModule>(SafeAssemblyLoader.LoadAll(dependencyContext).ToArray());
        }

#endif

        /// <summary>
        /// Registers all modules found in all assemblies.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterAllAssemblyModules(this ContainerBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.RegisterAssemblyModules(SafeAssemblyLoader.LoadAll().ToArray());
        }

        /// <summary>
        /// Registers all modules found in all assemblies.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IModuleRegistrar RegisterAllAssemblyModules<TModule>(this ContainerBuilder builder)
            where TModule : IModule
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            return builder.RegisterAssemblyModules<TModule>(SafeAssemblyLoader.LoadAll().ToArray());
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
