using System;

using Autofac.Builder;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Configures the component so instances are never disposed by the container.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterExternallyOwnedAttribute :
        RegisterBuilderAttribute
    {

        public override IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.ExternallyOwned();
        }

    }

}
