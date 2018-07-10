using Autofac;

namespace Cogito.Autofac.DependencyInjection
{

    public class AssemblyModule :
        Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterFromAttributes(typeof(AssemblyModule).Assembly);
        }

    }

}
