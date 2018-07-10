using Autofac;

namespace Cogito.Autofac
{

    public class ContainerModule :
        Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterSource(new OrderedRegistrationSource());
        }

    }

}
