using System;
using System.Collections.Generic;

using Autofac;
using Autofac.Core;

using Microsoft.Extensions.DependencyInjection;

namespace Cogito.Autofac.DependencyInjection
{

    class ServiceDescriptorComponentRegistration : IComponentRegistration
    {

        readonly IComponentRegistration parent;
        readonly ServiceDescriptor serviceDescriptor;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="serviceDescriptor"></param>
        public ServiceDescriptorComponentRegistration(IComponentRegistration parent, ServiceDescriptor serviceDescriptor)
        {
            this.parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.serviceDescriptor = serviceDescriptor ?? throw new ArgumentNullException(nameof(serviceDescriptor));
        }

        public Guid Id => parent.Id;

        public IInstanceActivator Activator => parent.Activator;

        public IComponentLifetime Lifetime => parent.Lifetime;

        public InstanceSharing Sharing => parent.Sharing;

        public InstanceOwnership Ownership => parent.Ownership;

        public IEnumerable<Service> Services => parent.Services;

        public IDictionary<string, object> Metadata => parent.Metadata;

        public IComponentRegistration Target => parent.Target;

        public event EventHandler<PreparingEventArgs> Preparing
        {
            add { parent.Preparing += value; }
            remove { parent.Preparing -= value; }
        }

        public event EventHandler<ActivatingEventArgs<object>> Activating
        {
            add { parent.Activating += value; }
            remove { parent.Activating -= value; }
        }

        public event EventHandler<ActivatedEventArgs<object>> Activated
        {
            add { parent.Activated += value; }
            remove { parent.Activated -= value; }
        }

        public void Dispose()
        {
            parent.Dispose();
        }

        public void RaiseActivated(IComponentContext context, IEnumerable<Parameter> parameters, object instance)
        {
            parent.RaiseActivated(context, parameters, instance);
        }

        public void RaiseActivating(IComponentContext context, IEnumerable<Parameter> parameters, ref object instance)
        {
            parent.RaiseActivating(context, parameters, ref instance);
        }

        public void RaisePreparing(IComponentContext context, ref IEnumerable<Parameter> parameters)
        {
            parent.RaisePreparing(context, ref parameters);
        }

        public override string ToString()
        {
            return parent.ToString();
        }

        public ServiceDescriptor ServiceDescriptor => serviceDescriptor;

    }

}
