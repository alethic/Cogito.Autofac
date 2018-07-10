using System;

using Autofac.Builder;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Configures the services that the component will provide.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterAsAttribute :
        RegisterTypeAttribute,
        IRegistrationBuilderAttribute
    {

        readonly Type[] types;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        public RegisterAsAttribute(Type type) :
            this(type != null ? new[] { type } : null)
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="types"></param>
        public RegisterAsAttribute(Type[] types)
        {
            this.types = types;
        }

        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return types != null && types.Length > 0 ? builder.As(types) : builder;
        }

    }

}
