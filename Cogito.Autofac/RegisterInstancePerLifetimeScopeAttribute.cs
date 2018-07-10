using System;

using Autofac.Builder;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Configure the component so that every dependent component or call to Resolve() within a single
    /// <see cref="ILifetimeScope"/> gets the same, shared instance. Dependent components in different lifetime scopes
    /// will get different instances.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterInstancePerLifetimeScopeAttribute :
        RegisterBuilderAttribute
    {

        readonly object[] lifetimeScopeTag;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RegisterInstancePerLifetimeScopeAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="lifetimeScopeTag"></param>
        public RegisterInstancePerLifetimeScopeAttribute(params object[] lifetimeScopeTag)
        {
            this.lifetimeScopeTag = lifetimeScopeTag;
        }

        public override IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            if (lifetimeScopeTag == null)
                return builder.InstancePerLifetimeScope();
            else
                return builder.InstancePerMatchingLifetimeScope(lifetimeScopeTag);
        }

    }

}
