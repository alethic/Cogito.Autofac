using Autofac;

namespace Cogito.Autofac
{

    public class AssemblyModule : ModuleBase
    {

        protected override void Register(ContainerBuilder builder)
        {
            builder.RegisterSource(new OrderedRegistrationSource());
        }

    }

}
