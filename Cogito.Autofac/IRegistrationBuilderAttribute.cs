using System;

using Autofac.Builder;

namespace Cogito.Autofac
{

    /// <summary>
    /// Implement this interface in a custom attribute to extend registration.
    /// </summary>
    public interface IRegistrationBuilderAttribute
    {

        /// <summary>
        /// Implement this method to handle registration.
        /// </summary>
        /// <typeparam name="TLimit"></typeparam>
        /// <typeparam name="TActivatorData"></typeparam>
        /// <typeparam name="TRegistrationStyle"></typeparam>
        /// <param name="type"></param>
        /// <param name="builder"></param>
        /// <returns></returns>
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder);

    }

}
