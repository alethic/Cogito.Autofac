using Autofac;

namespace Cogito.Autofac
{

    /// <summary>
    /// Base Autofac module class that enforces single registration.
    /// </summary>
    public abstract class ModuleBase : Module
    {

        /// <summary>
        /// Returns <c>true</c> if the module should be registered in the builder, or false if it already is.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        bool SingleRegistration(ContainerBuilder builder)
        {
            // check if we're already registered
            if (builder.Properties.ContainsKey(GetType().AssemblyQualifiedName))
                return false;

            // record ourselves as registered
            builder.Properties.Add(GetType().AssemblyQualifiedName, null);
            return true;
        }

        /// <summary>
        /// Registers objects in the container, once.
        /// </summary>
        /// <param name="builder"></param>
        protected sealed override void Load(ContainerBuilder builder)
        {
            if (!SingleRegistration(builder))
                return;

            Register(builder);
            base.Load(builder);
        }

        /// <summary>
        /// Overide to add registrations ot the container.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        protected abstract void Register(ContainerBuilder builder);

    }

}
