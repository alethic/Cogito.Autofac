using Autofac;

namespace FileAndServe.Autofac
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
