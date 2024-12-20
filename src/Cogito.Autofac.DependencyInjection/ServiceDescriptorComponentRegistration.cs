using System;
using System.Collections.Generic;

using Autofac.Core;
using Autofac.Core.Registration;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    class ServiceDescriptorComponentRegistration : ComponentRegistration
    {

        readonly ServiceDescriptor serviceDescriptor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="activator"></param>
        /// <param name="lifetime"></param>
        /// <param name="sharing"></param>
        /// <param name="ownership"></param>
        /// <param name="services"></param>
        /// <param name="metadata"></param>
        /// <param name="serviceDescriptor"></param>
        public ServiceDescriptorComponentRegistration(
            Guid id, 
            IInstanceActivator activator, 
            IComponentLifetime lifetime, 
            InstanceSharing sharing, 
            InstanceOwnership ownership,
            IEnumerable<Service> services,
            IDictionary<string, object> metadata,
            ServiceDescriptor serviceDescriptor) :
            base(
                id, 
                activator, 
                lifetime, 
                sharing, 
                ownership, 
                services, 
                metadata)
        {
            this.serviceDescriptor = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
        }

        public ServiceDescriptor ServiceDescriptor => serviceDescriptor;

    }

}
