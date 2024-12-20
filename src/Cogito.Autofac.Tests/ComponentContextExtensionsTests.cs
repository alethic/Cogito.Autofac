using System;
using System.Threading.Tasks;

using Autofac;

using FluentAssertions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cogito.Autofac.Tests
{

    [TestClass]
    public class ComponentContextExtensionsTests
    {

        class ScopeObject
        {

            public void Assert()
            {

            }

        }

        [TestMethod]
        public async Task Should_preserve_lifetimescope_through_nested_usings()
        {
            var b = new ContainerBuilder();
            b.Register(ctx => new ScopeObject()).InstancePerLifetimeScope();
            var c = b.Build();

            LifetimeScopeContext.CurrentLifetimeScope.Should().BeNull();

            using (var scope1 = c.EnterLifetimeScopeWithLogicalContext())
            {
                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
                var o1 = scope1.Resolve<ScopeObject>();
                await Task.Delay(TimeSpan.FromSeconds(1));
                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
                var o2 = scope1.Resolve<ScopeObject>();

                using (var scope2 = scope1.EnterLifetimeScopeWithLogicalContext())
                {
                    LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope2);
                    var o3 = scope2.Resolve<ScopeObject>();
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope2);
                    var o4 = scope2.Resolve<ScopeObject>();
                }

                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
            }

            LifetimeScopeContext.CurrentLifetimeScope.Should().BeNull();
        }

        [TestMethod]
        public async Task Should_discard_lifetimescope_on_exception()
        {
            var b = new ContainerBuilder();
            b.Register(ctx => new ScopeObject()).InstancePerLifetimeScope();
            var c = b.Build();

            LifetimeScopeContext.CurrentLifetimeScope.Should().BeNull();

            using (var scope1 = c.EnterLifetimeScopeWithLogicalContext())
            {
                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
                var o1 = scope1.Resolve<ScopeObject>();
                await Task.Delay(TimeSpan.FromSeconds(1));
                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
                var o2 = scope1.Resolve<ScopeObject>();

                try
                {
                    using (var scope2 = scope1.EnterLifetimeScopeWithLogicalContext())
                    {
                        LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope2);
                        var o3 = scope2.Resolve<ScopeObject>();
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope2);
                        var o4 = scope2.Resolve<ScopeObject>();
                        throw new Exception();
                    }
                }
                catch
                {

                }

                LifetimeScopeContext.CurrentLifetimeScope.Should().BeSameAs(scope1);
            }

            LifetimeScopeContext.CurrentLifetimeScope.Should().BeNull();
        }

    }

}
