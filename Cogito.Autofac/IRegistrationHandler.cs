using System;
using System.Collections.Generic;

using Autofac;

namespace Cogito.Autofac
{

    /// <summary>
    /// Describes a method of registering by attribute.
    /// </summary>
    public interface IRegistrationHandler
    {

        /// <summary>
        /// Registers the type using the standard registration flow.
        /// </summary>
        /// <param name="attributes"></param>
        void Register(ContainerBuilder builder, Type type, IEnumerable<IRegistrationRootAttribute> attributes);

    }

}