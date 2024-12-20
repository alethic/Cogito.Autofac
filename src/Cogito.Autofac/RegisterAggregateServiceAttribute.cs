using System;

namespace Cogito.Autofac
{

    /// <summary>
    /// Aggregates multiple related components into a single interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class RegisterAggregateServiceAttribute :
        Attribute,
        IRegistrationRootAttribute
    {

        /// <summary>
        /// Uses the standard <see cref="RegisterAggregateServiceHandler"/>.
        /// </summary>
        public Type HandlerType => typeof(RegisterAggregateServiceHandler);

    }

}
