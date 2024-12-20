using System;

using Autofac;

namespace Cogito.Autofac
{

    public static class ComponentContextExtensions
    {

        /// <summary>
        /// Invokes a delegate upon disposal.
        /// </summary>
        class DelegateDisposable : IDisposable
        {

            readonly Action dispose;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="dispose"></param>
            public DelegateDisposable(Action dispose)
            {
                this.dispose = dispose ?? throw new ArgumentNullException(nameof(dispose));
            }

            /// <summary>
            /// Disposes of the instance.
            /// </summary>
            public void Dispose()
            {
                dispose();
            }

        }

        /// <summary>
        /// Begin a new nested scope with a logical call context. The created <see cref="ILifetimeScope"/> remains the
        /// current async-local context until disposed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ILifetimeScope EnterLifetimeScopeWithLogicalContext(this IComponentContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var prev = LifetimeScopeContext.CurrentLifetimeScope;
            var next = context.Resolve<ILifetimeScope>().BeginLifetimeScope(b => ConfigureBuilder(b, prev, null));
            return LifetimeScopeContext.CurrentLifetimeScope = next;
        }

        /// <summary>
        /// Begin a new nested scope with a logical call context. The created <see cref="ILifetimeScope"/> remains the
        /// current async-local context until disposed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static ILifetimeScope EnterLifetimeScopeWithLogicalContext(this IComponentContext context, object tag)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (tag == null)
                throw new ArgumentNullException(nameof(tag));

            var prev = LifetimeScopeContext.CurrentLifetimeScope;
            var next = context.Resolve<ILifetimeScope>().BeginLifetimeScope(tag, b => ConfigureBuilder(b, prev, null));
            return LifetimeScopeContext.CurrentLifetimeScope = next;
        }

        /// <summary>
        /// Begin a new nested scope with a logical call context. The created <see cref="ILifetimeScope"/> remains the
        /// current async-local context until disposed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public static ILifetimeScope EnterLifetimeScopeWithLogicalContext(this IComponentContext context, Action<ContainerBuilder> configurationAction)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (configurationAction == null)
                throw new ArgumentNullException(nameof(configurationAction));

            var prev = LifetimeScopeContext.CurrentLifetimeScope;
            var next = context.Resolve<ILifetimeScope>().BeginLifetimeScope(b => ConfigureBuilder(b, prev, configurationAction));
            return LifetimeScopeContext.CurrentLifetimeScope = next;
        }

        /// <summary>
        /// Begin a new nested scope with a logical call context. The created <see cref="ILifetimeScope"/> remains the
        /// current async-local context until disposed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="configurationAction"></param>
        /// <returns></returns>
        public static ILifetimeScope EnterLifetimeScopeWithLogicalContext(this IComponentContext context, object tag, Action<ContainerBuilder> configurationAction)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (configurationAction == null)
                throw new ArgumentNullException(nameof(configurationAction));

            var prev = LifetimeScopeContext.CurrentLifetimeScope;
            var next = context.Resolve<ILifetimeScope>().BeginLifetimeScope(tag, b => ConfigureBuilder(b, prev, configurationAction));
            return LifetimeScopeContext.CurrentLifetimeScope = next;
        }

        /// <summary>
        /// Applies configuration to the builder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="previous"></param>
        /// <param name="configurationAction"></param>
        static void ConfigureBuilder(ContainerBuilder builder, ILifetimeScope previous, Action<ContainerBuilder> configurationAction)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.RegisterInstance(new DelegateDisposable(() => RestoreLifetimeScope(previous)));
            configurationAction?.Invoke(builder);
        }

        /// <summary>
        /// Invoked when a lifetime scope is disposed.
        /// </summary>
        static void RestoreLifetimeScope(ILifetimeScope previous)
        {
            LifetimeScopeContext.CurrentLifetimeScope = previous;
        }

    }

}
