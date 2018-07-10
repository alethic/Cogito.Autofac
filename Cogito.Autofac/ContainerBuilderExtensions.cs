using System;
using System.Linq;

using Autofac;
using Autofac.Builder;
using Autofac.Core.Registration;

using Microsoft.Extensions.Configuration;

using FileAndServe.Configuration;
using FileAndServe.Reflection;

namespace FileAndServe.Autofac
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
        /// Registers a type provided by the <see cref="IConfigurationRoot"/> at the given path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<T, SimpleActivatorData, SingleRegistrationStyle> RegisterConfigurationBinding<T>(this ContainerBuilder builder, string path)
            where T : class, new()
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return builder.Register(ctx => ctx.Resolve<IConfigurationRoot>().Bind<T>(path));
        }

        /// <summary>
        /// Registers a type provided by the <see cref="IConfigurationRoot"/> at the given path.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle> RegisterConfigurationBinding(this ContainerBuilder builder, Type type, string path)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            return builder.Register(ctx => ctx.Resolve<IConfigurationRoot>().Bind(type, path)).As(type);
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
