using System;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Base registration root attribute that uses the standard RegisterType/RegisterGenericType flow.
    /// </summary>
    public class RegisterTypeAttribute :
        Attribute,
        IRegistrationRootAttribute
    {

        /// <summary>
        /// Uses the standard <see cref="RegisterTypeHandler"/>.
        /// </summary>
        public Type HandlerType => typeof(RegisterTypeHandler);

    }

}
