using System;

using Autofac.Builder;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Provides a key that can be used to retrieve the component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterKeyedAttribute :
        RegisterTypeAttribute,
        IRegistrationBuilderAttribute
    {

        readonly Type type;
        readonly object serviceKey;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceKey"></param>
        public RegisterKeyedAttribute(object serviceKey)
        {
            this.serviceKey = serviceKey;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="serviceKey"></param>
        public RegisterKeyedAttribute(Type type, object serviceKey) :
            this(serviceKey)
        {
            this.type = type;
        }

        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.Keyed(serviceKey, this.type ?? type);
        }

    }

}
