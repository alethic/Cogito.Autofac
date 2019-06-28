using System;
using System.Collections.Generic;
using System.Linq;
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
            b.Populate(s => s.Should().HaveCount(8));
            var c = b.Build();
        }

        [TestMethod]
        public void Should_be_able_to_find_autofac_service()
        {
            var b = new ContainerBuilder();
            b.RegisterType<TestOptions>();
            b.Populate(s => s.Should().ContainSingle(i => i.ServiceType == typeof(TestOptions)));
            var c = b.Build();
            c.Resolve<TestOptions>();
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

        [TestMethod]
        public void Can_remove_descriptor_added_in_same_pass()
        {
            var b = new ContainerBuilder();
            b.Populate(s =>
            {
                var d = ServiceDescriptor.Singleton(new object());
                s.Add(d);
                s.Remove(d).Should().BeTrue();
            });
            var c = b.Build();
            c.ResolveOptional<object>().Should().BeNull();
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void Should_fail_when_trying_to_remove_service_registered_elsewhere()
        {
            var d = ServiceDescriptor.Singleton(new object());
            var b = new ContainerBuilder();
            b.Populate(s => s.Add(d));
            b.Populate(s => s.Remove(d));
            var c = b.Build();
        }

        class TestOptions
        {

            public string Value { get; set; }

        }

    }

}
