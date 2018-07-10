using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac;
using Autofac.Builder;

namespace FileAndServe.Autofac
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
        public virtual void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes)
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
                type.GetCustomAttributes()
                    .OfType<IRegistrationBuilderAttribute>()
                    .Concat(GetBuilders(type)));
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
            if (type.GetTypeInfo().IsGenericType)
                ApplyBuilders(type, builder.RegisterGeneric(type), attributes, builders);
            else
                ApplyBuilders(type, builder.RegisterType(type), attributes, builders);
        }

        /// <summary>
        /// Yields attional builders to apply.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected virtual IEnumerable<IRegistrationBuilderAttribute> GetBuilders(Type type)
        {
            yield break;
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
            IEnumerable<IRegistrationRootAttribute> attributes,
            IEnumerable<IRegistrationBuilderAttribute> builders)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            if (builders == null)
                throw new ArgumentNullException(nameof(builders));

            // apply each builder in order
            foreach (var b in builders)
                builder = b.Build(type, builder);

            return builder;
        }

    }

}
