using System;
using System.Collections.Generic;

using Autofac;
using Autofac.Extras.AggregateService;

namespace Cogito.Autofac
{

    /// <summary>
    /// Registration handler to aggregate multiple related components into a single interface.
    /// </summary>
    public class RegisterAggregateServiceHandler :
        IRegistrationHandler
    {

        /// <summary>
        /// Registers the aggregate service.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="type"></param>
        /// <param name="attributes"></param>
        public void Register(
            ContainerBuilder builder,
            Type type,
            IEnumerable<IRegistrationRootAttribute> attributes)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            builder.RegisterAggregateService(type);
        }

    }

}
