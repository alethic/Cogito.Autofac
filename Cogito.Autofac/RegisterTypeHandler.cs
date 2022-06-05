using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;

using Cogito.Linq;

namespace Cogito.Autofac
{

    /// <summary>
    /// Conducts registration using the standard RegisterType/RegisterGeneric flow.
    /// </summary>
    public class RegisterTypeHandler :
        IRegistrationHandler
    {

        /// <summary>
        /// Registers the type using the standard registration flow.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        public virtual void Register(
            ContainerBuilder builder,
            Type type,
            IEnumerable<IRegistrationRootAttribute> attributes)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Register(
                builder,
                type,
                attributes,
                GetBuilders(type));
        }

        /// <summary>
        /// Carries out the registration against the container.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        /// <param name="builders"></param>
        protected virtual void Register(
            ContainerBuilder builder,
            Type type,
            IEnumerable<IRegistrationRootAttribute> attributes,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            foreach (var attribute in attributes)
                Register(builder, type, attribute, builders);
        }

        /// <summary>
        /// Carries out the registration against the container.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <param name="builders"></param>
        protected virtual void Register(
            ContainerBuilder builder,
            Type type,
            IRegistrationRootAttribute attribute,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            // root is itself a builder add it to builder list
            if (attribute is IRegistrationBuilderAttribute a)
#if NET461
                builders = AppendInternal(builders, a);
#else
                builders = builders.Append(a);
#endif

            // core registration method with final set of builders
            RegisterCore(builder, type, attribute, builders);
        }

#if NET461
        static IEnumerable<T> AppendInternal<T>(IEnumerable<T> source, T value)
        {
            foreach (var i in source)
                yield return i;

            yield return value;
        }
#endif

        /// <summary>
        /// Carries out the registration against the container and applies the builders.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="attribute"></param>
        /// <param name="builders"></param>
        protected virtual void RegisterCore(
            ContainerBuilder builder,
            Type type,
            IRegistrationRootAttribute attribute,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            if (type.GetTypeInfo().IsGenericType)
                ApplyBuilders(type, builder.RegisterGeneric(type), attribute, builders);
            else
                ApplyBuilders(type, builder.RegisterType(type), attribute, builders);
        }

        /// <summary>
        /// Yields additional builders to apply.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IRegistrationBuilderAttribute> GetBuilders(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));

            return type.GetCustomAttributes()
                .OfType<IRegistrationBuilderAttribute>()
                .Where(i => i as IRegistrationRootAttribute == null);
        }

        /// <summary>
        /// Applies the detected set of builders.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="type"></param>
        /// <param name="builder"></param>
        /// <param name="attribute"></param>
        /// <param name="builders"></param>
        /// <returns></returns>
        protected virtual IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> ApplyBuilders<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder,
            IRegistrationRootAttribute attribute,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            // apply each builder in order
            foreach (var b in builders.Distinct())
                builder = b.Build(type, builder);

            return builder;
        }

    }

}
