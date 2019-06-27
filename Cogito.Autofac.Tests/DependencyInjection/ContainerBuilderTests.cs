using System.Collections.Generic;

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
        public void Should_return_descriptor_for_external_service()
        {
            var b = new ContainerBuilder();
            b.RegisterInstance(new object());
            b.Populate(s => s.AddSingleton(new object()));
            b.Populate(s => s.Should().HaveCount(5));
            var c = b.Build();
        }

        [TestMethod]
        public void Should_allow_open_generics_and_multiple_instance_registrations()
        {
            var b = new ContainerBuilder();
            b.Populate(s => s.AddOptions());
            b.Populate(s => s.Configure<TestOptions>(a => a.Value = "Hello").Configure<TestOptions>(a => a.Value = "Goodbye"));
            var c = b.Build();

            var o = c.Resolve<IOptions<TestOptions>>();
            var z = c.Resolve<IOptionsSnapshot<TestOptions>>();
            var m = c.Resolve<IOptionsMonitor<TestOptions>>();
            o.Should().NotBeNull();
            o.Value.Value.Should().Be("Goodbye");
            z.Should().NotBeNull();
            m.Should().NotBeNull();

            var l = c.Resolve<IEnumerable<IOptions<TestOptions>>>();
            l.Should().HaveCount(1);
        }

        class TestOptions
        {

            public string Value { get; set; }

        }

    }

}
