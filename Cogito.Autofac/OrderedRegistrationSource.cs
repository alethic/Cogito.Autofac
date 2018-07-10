using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Autofac.Builder;
using Autofac.Core;

using FileAndServe.Core.Reflection;

namespace FileAndServe.Autofac
{

    /// <summary>
    /// Provides support for <see cref="IOrderedEnumerable{TElement}"/>.
    /// </summary>
    public class OrderedRegistrationSource : IRegistrationSource
    {

        /// <summary>
        /// Retrieve registrations for an unregistered service, to be used
        /// by the container.
        /// </summary>
        /// <param name="service">The service that was requested.</param>
        /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
        /// <returns>Registrations providing the service.</returns>
        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var typedService = service as IServiceWithType;
            if (typedService != null)
            {
                var serviceType = typedService.ServiceType;
                if (serviceType.IsInstanceOfGenericType(typeof(IOrderedEnumerable<>)))
                {
                    var dependencyType = serviceType.GenericTypeArguments.Single();

                    var registration = (IComponentRegistration)CreateRegistrationMethod
                        .MakeGenericMethod(dependencyType)
                        .Invoke(null, new object[0]);

                    return new[] { registration };
                }
            }

            return Enumerable.Empty<IComponentRegistration>();
        }

        /// <summary>
        /// Gets whether the registrations provided by this source are 1:1 adapters on top
        /// of other components (ie. like Meta, Func, or Owned.)
        /// </summary>
        /// <remarks>Always returns false.</remarks>
        public bool IsAdapterForIndividualComponents => false;

        /// <summary>
        /// Helper method to return registrations.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        static IComponentRegistration CreateOrderedRegistration<TService>()
        {
            return RegistrationBuilder
                .ForDelegate((c, ps) => c.ResolveOrdered<TService>(ps))
                .ExternallyOwned()
                .CreateRegistration();
        }

        static readonly MethodInfo CreateRegistrationMethod = typeof(OrderedRegistrationSource).GetRuntimeMethods().Single(m => m.Name == nameof(CreateOrderedRegistration));

        internal const string OrderingMetadataKey = "AutofacOrderingMetadataKey";

    }

}
