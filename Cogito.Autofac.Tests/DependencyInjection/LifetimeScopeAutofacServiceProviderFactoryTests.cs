using Autofac;
using Autofac.Core.Registration;

using Cogito.Autofac.DependencyInjection;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Autofac.Tests.DependencyInjection
{

    [TestClass]
    public class LifetimeScopeAutofacServiceProviderFactoryTests
    {

        class SingletonTestType
        {

        }

        class InstanceTestType
        {

        }

        class InnerTestType
        {

        }

        class InnerTestType2
        {

        }

        [TestMethod]
        public void Can_create_service_provider_for_scope()
        {
            var c = new global::Autofac.ContainerBuilder();
            c.RegisterType(typeof(SingletonTestType)).SingleInstance();
            c.RegisterType(typeof(InstanceTestType));

            var container = c.Build();
            var singleton = container.Resolve<SingletonTestType>();
            var instance = container.Resolve<InstanceTestType>();
            container.Invoking(a => a.Resolve<InnerTestType>()).Should().Throw<ComponentNotRegisteredException>();
            container.Invoking(a => a.Resolve<InnerTestType2>()).Should().Throw<ComponentNotRegisteredException>();

            var factory = new LifetimeScopeAutofacServiceProviderFactory(container, b => b.RegisterType<InnerTestType2>());
            var inner = factory.CreateBuilder(new ServiceCollection());
            inner.Configure(b => b.RegisterType<InnerTestType>());
            var provider = factory.CreateServiceProvider(inner);

            var singleton2 = provider.GetRequiredService<SingletonTestType>();
            singleton2.Should().BeSameAs(singleton);

            var instance2 = provider.GetRequiredService<InstanceTestType>();
            instance2.Should().NotBeSameAs(instance);

            var innerTest = provider.GetRequiredService<InnerTestType>();
            innerTest.Should().NotBeNull();

            var innerTest2 = provider.GetRequiredService<InnerTestType2>();
            innerTest2.Should().NotBeNull();


        }

    }

}
