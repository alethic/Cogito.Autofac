using System;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Describes an attribute that provides a custom registration flow.
    /// </summary>
    public interface IRegistrationRootAttribute
    {

        /// <summary>
        /// Gets the type of the object to conduct the registration.
        /// </summary>
        Type HandlerType { get; }

    }

}
