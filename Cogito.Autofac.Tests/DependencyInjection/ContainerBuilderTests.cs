using Autofac;
using Autofac.Extensions.DependencyInjection;

using Cogito.Autofac.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Autofac.Tests.DependencyInjection
{

    [TestClass]
    public class ContainerBuilderTests
    {

        [TestMethod]
        public void Foo()
        {
            var b = new ContainerBuilder();
            b.Populate(s => s.AddOptions());
            b.Populate(s => s.Configure<TestOptions>(a => a.Value = true));
            var c = b.Build();

            var o = c.Resolve<IOptions<TestOptions>>();
            var z = c.Resolve<IOptionsSnapshot<TestOptions>>();
            var m = c.Resolve<IOptionsMonitor<TestOptions>>();

            o.Should().NotBeNull();
            z.Should().NotBeNull();
            m.Should().NotBeNull();
        }

        class TestOptions
        {

            public bool Value { get; set; }

        }

    }

}
