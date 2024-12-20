using System.Threading;

using Autofac;

namespace Cogito.Autofac
{

    /// <summary>
    /// Provides access to the current <see cref="ILifetimeScope"/> context.
    /// </summary>
    public static class LifetimeScopeContext
    {

        static readonly AsyncLocal<ILifetimeScope> LogicalCallContext = new AsyncLocal<ILifetimeScope>();

        /// <summary>
        /// Gets or sets the current scope.
        /// </summary>
        public static ILifetimeScope CurrentLifetimeScope
        {
            get => LogicalCallContext.Value;
            set => LogicalCallContext.Value = value;
        }

    }

}
