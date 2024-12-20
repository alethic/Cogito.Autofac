using System;
using Autofac.Builder;

namespace Cogito.Autofac
{

    /// <summary>
    /// Describes a registration attribute that implements additional registration builder functionality.
    /// </summary>
    public abstract class RegisterBuilderAttribute :
        Attribute,
        IRegistrationBuilderAttribute
    {

        /// <summary>
        /// Implement this method to extend registration.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="type"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        public abstract IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder);

    }

}
