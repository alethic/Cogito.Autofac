using System;

using Autofac.Builder;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Configure the component so that every dependent component or call to Resolve() gets the same, shared instance.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterSingleInstanceAttribute :
        RegisterBuilderAttribute
    {

        public override IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            return builder.SingleInstance();
        }

    }

}
