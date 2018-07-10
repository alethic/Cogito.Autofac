using System;

using Autofac.Builder;

namespace Cogito.Autofac
{

    /// <summary>
    /// Provides a textual name that can be used to retrieve the component.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class RegisterNamedAttribute :
        RegisterTypeAttribute,
        IRegistrationBuilderAttribute
    {

        readonly Type type;
        readonly string name;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        public RegisterNamedAttribute(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public RegisterNamedAttribute(Type type, string name) :
            this(name)
        {
            this.type = type;
        }

        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.Named(name, this.type ?? type);
        }

    }

}
