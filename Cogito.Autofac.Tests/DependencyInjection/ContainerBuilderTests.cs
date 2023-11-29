using System;
using System.Collections.Generic;
using System.Linq;

using Autofac;

using Cogito.Autofac.DependencyInjection;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;


#if NETCOREAPP3_1_OR_GREATER
using Microsoft.AspNetCore.Routing;
#endif

namespace Cogito.Autofac.Tests.DependencyInjection
{

    [TestClass]
    public class ContainerBuilderTests
    {

        [TestMethod]
        public void Should_return_descriptor_for_external_service()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterInstance(new object());
            b.Populate(s => s.AddSingleton(new object()));
            var c = b.Build();
            c.ComponentRegistry.Registrations.Should().HaveCount(6);
        }

        [TestMethod]
        public void Should_preserve_instance_descriptor()
        {
            var o = new object();
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterInstance(o);
            b.Populate(s => s.Single(i => i.ImplementationInstance == o).ImplementationInstance.Should().BeSameAs(o));
            var c = b.Build();
        }

        [TestMethod]
        public void Should_be_able_to_find_autofac_service()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterType<TestOptions>();
            b.Populate(s => s.Should().ContainSingle(i => i.ServiceType == typeof(TestOptions)));
            var c = b.Build();
            c.Resolve<TestOptions>();
        }

        [TestMethod]
        public void Should_retrieve_last()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddSingleton(new TestOptions() { Value = "a" }));
            b.Populate(s => s.AddSingleton(new TestOptions() { Value = "b" }));
            var c = b.Build();

            var l = c.Resolve<TestOptions>();
            l.Value.Should().Be("b");
        }

        [TestMethod]
        public void Should_retrieve_last_af()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterInstance(new TestOptions() { Value = "a" });
            b.RegisterInstance(new TestOptions() { Value = "b" });
            var c = b.Build();

            var l = c.Resolve<TestOptions>();
            l.Value.Should().Be("b");
        }

        [TestMethod]
        public void Can_remove_descriptor_added_in_same_pass()
        {
            var b = new global::Autofac.ContainerBuilder();
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
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.Add(d));
            b.Populate(s => s.Remove(d));
            var c = b.Build();
        }

        [TestMethod]
        public void Can_register_scoped()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddScoped(typeof(TestOptions)));
            var c = b.Build();
            var a = c.Resolve<TestOptions>();

            var scope = c.BeginLifetimeScope();
            var z = scope.Resolve<TestOptions>();

            a.Should().NotBeSameAs(z);
        }

        [TestMethod]
        public void Can_register_singleton_and_get_in_scope()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddSingleton(typeof(TestOptions)));
            var c = b.Build();
            var a = c.Resolve<TestOptions>();

            var scope = c.BeginLifetimeScope();
            var z = scope.Resolve<TestOptions>();

            a.Should().BeSameAs(z);
        }

        [TestMethod]
        public void Can_register_options_and_get_in_scope()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.Configure<TestOptions>(o => o.Value = "Set"));
            var c = b.Build();
            var a = c.Resolve<IOptions<TestOptions>>();

            var scope = c.BeginLifetimeScope();
            var z = scope.Resolve<IOptions<TestOptions>>();

            a.Should().BeSameAs(z);
        }

        [TestMethod]
        public void Can_register_options_and_get_in_scope_with_builder()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddSingleton<TestOptions>());
            var c = b.Build();
            var a = c.Resolve<TestOptions>();

            var scope = c.BeginLifetimeScope(builder => { });
            var z = scope.Resolve<TestOptions>();
        }

        [TestMethod]
        public void Can_register_options_and_get_in_scope_with_builder2()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterType<TestOptions>().SingleInstance();
            var c = b.Build();
            var a = c.Resolve<TestOptions>();

            var scope = c.BeginLifetimeScope(builder => { });
            var z = scope.Resolve<TestOptions>();
        }

        [TestMethod]
        public void Should_not_add_when_tryadd()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterType<TestOptions>().SingleInstance();
            b.Populate(s => s.TryAddSingleton<TestOptions>());
            var c = b.Build();
            var o = c.Resolve<IEnumerable<TestOptions>>();
            o.Should().HaveCount(1);
        }

        [TestMethod]
        public void Should_add_when_add_after_tryadd()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => { s.AddSingleton<TestOptions, TestOptionsA>(); s.AddSingleton<TestOptions, TestOptionsB>(); });
            var c = b.Build();
            var l = c.Resolve<IEnumerable<TestOptions>>().ToList();
            l.Should().ContainItemsAssignableTo<TestOptionsA>();
            l.Should().ContainItemsAssignableTo<TestOptionsB>();
            var o = c.Resolve<TestOptions>();
            o.GetType().Should().Be(typeof(TestOptionsB));
        }

        [TestMethod]
        public void Should_add_when_add_after_tryadd_across_calls()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.TryAddSingleton<TestOptions, TestOptionsA>());
            b.Populate(s => s.AddSingleton<TestOptions, TestOptionsB>());
            var c = b.Build();
            var o = c.Resolve<TestOptions>();
            o.GetType().Should().Be(typeof(TestOptionsB));
        }

        [TestMethod]
        public void Should_not_add_when_tryaddenumerable_with_same_instance()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterType<TestOptions>().SingleInstance();
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(TestOptions))));
            var c = b.Build();
            var o = c.Resolve<IEnumerable<TestOptions>>();
            o.Should().HaveCount(1);
        }

        [TestMethod]
        public void Should_add_when_tryaddenumerable_with_different_instance()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.RegisterType<TestOptions>().As<object>().SingleInstance();
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(object), typeof(TestOptions1))));
            var c = b.Build();
            var o = c.Resolve<IEnumerable<object>>();
            o.Should().HaveCount(2);
        }

        [TestMethod]
        public void Should_not_add_when_tryaddenumerable_with_same_factory_type()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Transient<TestOptions, TestOptionsA>(svc => new TestOptionsA())));
            b.RegisterInstance(new TestOptions2());
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Transient<TestOptions, TestOptionsA>(svc => new TestOptionsA())));
            b.RegisterInstance(new TestOptions2());
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Transient<TestOptions, TestOptionsB>(svc => new TestOptionsB())));
            b.RegisterInstance(new TestOptions2());
            b.Populate(s => s.TryAddEnumerable(ServiceDescriptor.Transient<TestOptions, TestOptionsB>(svc => new TestOptionsB())));
            var c = b.Build();
            var o = c.Resolve<IEnumerable<TestOptions>>();
            o.Should().HaveCount(2);
        }

        [TestMethod]
        public void Should_not_insert_duplicate_options()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddOptions());
            b.Populate(s => s.AddOptions());
            var c = b.Build();
            var z = c.Resolve<IEnumerable<IOptionsFactory<object>>>();
            z.Should().HaveCount(1);
        }

#if NETCOREAPP3_1_OR_GREATER

        [TestMethod]
        public void Should_not_insert_duplicate_route_options()
        {
            var b = new global::Autofac.ContainerBuilder();
            b.Populate(s => s.AddOptions());
            b.Populate(s => s.AddHealthChecks());
            b.Populate(s => s.AddCors());
            b.Populate(s => s.AddMvc());
            b.Populate(s => s.AddRouting());
            b.Populate(s => s.AddRouting());
            b.Populate(s => s.AddMvc());
            var c = b.Build();
            var z = c.Resolve<IEnumerable<IConfigureOptions<RouteOptions>>>();
            z.Should().HaveCount(2);
            var e = c.Resolve<EndpointDataSource>();
            var o = c.Resolve<IOptions<RouteOptions>>();
        }

#endif

        class TestOptions
        {

            public string Value { get; set; }

        }

        class TestOptionsA : TestOptions
        {



        }

        class TestOptionsB : TestOptions
        {



        }

        class TestOptions1
        {

            public string Value { get; set; }

        }

        class TestOptions2
        {

            public string Value { get; set; }

        }

    }

}
