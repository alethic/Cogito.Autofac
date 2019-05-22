using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;

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
        /// <param name="builders"></param>
        protected virtual void Register(
            ContainerBuilder builder,
            Type type,
            IEnumerable<IRegistrationRootAttribute> attributes,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
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
        /// <param name="builders"></param>
        protected virtual void Register(
            ContainerBuilder builder,
            Type type,
            IRegistrationRootAttribute attribute,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (attribute == null)
                throw new ArgumentNullException(nameof(attribute));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            // apply root attribute first
            if (attribute is IRegistrationBuilderAttribute a)
                builders = builders.Prepend(a);

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
            return type.GetCustomAttributes()
                .OfType<IRegistrationBuilderAttribute>()
                .Where(i => !i.GetType().IsAssignableTo<IRegistrationRootAttribute>());
        }

        /// <summary>
        /// Applies the detected set of builders.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="builder"></param>
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
            foreach (var b in builders)
                builder = b.Build(type, builder);

            return builder;
        }

    }

}
