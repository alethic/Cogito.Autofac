using System;

using Autofac.Builder;
using Autofac.Features.AttributeFilters;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Applies attribute-based filtering on constructor dependencies.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RegisterWithAttributeFilteringAttribute :
        RegisterBuilderAttribute
    {

        public override IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> Build<TLimit, TActivatorData, TRegistrationStyle>(
            Type type,
            IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder)
        {
            var b = builder as IRegistrationBuilder<TLimit, ReflectionActivatorData, TRegistrationStyle>;
            if (b != null)
                b = b.WithAttributeFiltering();

            return (IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle>)b;
        }

    }

}
