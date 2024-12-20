using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;

namespace Cogito.Autofac
{

    public static class ContainerBuilderAttributeExtensions
    {

        /// <summary>s
        /// Registers all types by their decorated registration attributes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        public static void RegisterFromAttributes(this ContainerBuilder builder, params Assembly[] assemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            builder.RegisterFromAttributes((IEnumerable<Assembly>)assemblies);
        }

        /// <summary>s
        /// Registers all types by their decorated registration attributes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="assemblies"></param>
        public static void RegisterFromAttributes(this ContainerBuilder builder, IEnumerable<Assembly> assemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (assemblies == null)
                throw new ArgumentNullException(nameof(assemblies));

            builder.RegisterFromAttributes(assemblies.SelectMany(i => GetAssemblyTypesSafe(i)));
        }

        /// <summary>
        /// Registers all specified types by their decorated registration attributes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="types"></param>
        public static void RegisterFromAttributes(this ContainerBuilder builder, params Type[] types)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            RegisterFromAttributes(builder, (IEnumerable<Type>)types);
        }

        /// <summary>
        /// Registers all specified types by their decorated registration attributes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="types"></param>
        public static void RegisterFromAttributes(this ContainerBuilder builder, IEnumerable<Type> types)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            foreach (var type in OrderByPriority(types))
                builder.RegisterFromAttributes(type);
        }

        /// <summary>
        /// Orders the types to register based on their priority.
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        static IEnumerable<Type> OrderByPriority(IEnumerable<Type> types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            return types.OrderByDescending(i => GetPriority(i));
        }

        /// <summary>
        /// Gets the priority of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        static int GetPriority(Type type)
        {
            return type.GetCustomAttribute<RegisterPriorityAttribute>()?.Priority ?? 0;
        }

        /// <summary>
        /// Registers the specified type by it's decorated registration attributes.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        public static void RegisterFromAttributes(this ContainerBuilder builder, Type type)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            // group by unique registration handler types
            var items = type.GetTypeInfo()
                .GetCustomAttributes(typeof(IRegistrationRootAttribute), false)
                .OfType<IRegistrationRootAttribute>()
                .Where(i => i.HandlerType?.IsAssignableTo<IRegistrationHandler>() ?? false)
                .GroupBy(i => i.HandlerType)
                .Select(i => new
                {
                    Handler = (IRegistrationHandler)Activator.CreateInstance(i.Key),
                    Attributes = i
                });

            // dispatch to associated handler
            foreach (var item in items)
                item.Handler.Register(builder, type, item.Attributes);
        }

        /// <summary>
        /// Catch exceptions when loading types.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        static ICollection<Type> GetAssemblyTypesSafe(this Assembly assembly)
        {
            var l = new List<Type>();

            try
            {
                l = assembly.GetTypes()
                    .Where(i => i != null)
                    .ToList();
            }
            catch (ReflectionTypeLoadException e)
            {
                foreach (var t in e.Types)
                {
                    try
                    {
                        if (t != null)
                            l.Add(t);
                    }
                    catch (BadImageFormatException)
                    {
                        // ignore
                    }
                }
            }

            return l;
        }

    }

}
