using System;
using System.Collections.Generic;

using NUnit.Framework;


namespace Forest.Tests
{
    internal static class Inner
    {
        internal const string ViewName = "Inner";
        internal class ViewModel { }

        [View(ViewName)]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
                //throw new NotImplementedException();
            }
        }
    }

    internal static class Outer
    {
        internal const string ViewName = "Outer";
        internal class ViewModel { }

        [View(ViewName)]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
                var innerView = (Inner.View) FindRegion("SomeRegion").ActivateView(Inner.ViewName);
                innerView.ViewModel = new Inner.ViewModel();
            }
        }
    }

    [TestFixture]
    public class TestClass
    {
        private static IDictionary<string, object> Add(IDictionary<string, object> target, string str)
        {
            target.Add(str, new Dictionary<string, object>());
            return target;
        }
        private static IDictionary<string, object> Get(IDictionary<string, object> target, string str)
        {
            return target[str] as IDictionary<string, object>;
        }

        [Test]
        public void TestMethod()
        {
            var ctx = new DefaultForestContext(new View.Factory(), null);
            ctx.ViewRegistry.Register<Inner.View>();
            var state = Engine.Update(ctx, ForestOperation.NewInstantiateView(Identifier.Shell, string.Empty, Inner.ViewName), State.Empty);

            Assert.AreNotEqual(state, State.Empty);
        }

        [Test]
        public void TestAddingViewFormAnotherOne()
        {
            var ctx = new DefaultForestContext(new View.Factory(), null);
            ctx.ViewRegistry.Register<Inner.View>();
            ctx.ViewRegistry.Register<Outer.View>();
            var state = Engine.Update(ctx, ForestOperation.NewInstantiateView(Identifier.Shell, string.Empty, Outer.ViewName), State.Empty);

            Assert.AreNotEqual(state, State.Empty);
        }
    }
}
