using System;
using System.Collections.Generic;

using Autofac.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    /// <summary>
    /// Wraps an existing <see cref="IRegistrationSource"/> and maintains the set of applied <see cref="ServiceDescriptor"/>s.
    /// </summary>
    class ServiceDescriptorRegistrationSource : IRegistrationSource
    {

        readonly IRegistrationSource parent;
        readonly ServiceDescriptor[] serviceDescriptor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="serviceDescriptor"></param>
        public ServiceDescriptorRegistrationSource(IRegistrationSource parent, ServiceDescriptor[] serviceDescriptor)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.serviceDescriptor = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
        }

        public bool IsAdapterForIndividualComponents => false;

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrations)
        {
            return parent.RegistrationsFor(service, registrations);
        }

        public override string ToString()
        {
            return parent.ToString();
        }

        /// <summary>
        /// Gets the originally assigned descriptor.
        /// </summary>
        public ServiceDescriptor[] ServiceDescriptors => serviceDescriptor;

    }

}
