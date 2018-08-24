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
                var region = FindRegion("SomeRegion");
                var count = new Random().Next(1, 10);
                for (var i = 0; i < count; i++)
                {
                    var innerView = (Inner.View) region.ActivateView(Inner.ViewName);
                    innerView.ViewModel = new Inner.ViewModel();
                }
                var count2 = new Random().Next(1, 5);
                for (var i = 0; i < count2; i++)
                {
                    region.ActivateView(Outer.ViewName);
                }
            }
        }
    }

    internal class Visitor : IStateVisitor
    {
        public void BFS(HierarchyKey key, string region, string view, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine(">> BFS: {0} : {1}", index, key.Hash);
        }

        public void DFS(HierarchyKey key, string region, string view, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine("<< DFS: {0} : {1}", index, key.Hash);
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

            var state = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.Shell, string.Empty, Inner.ViewName), State.Empty);

            Assert.AreNotEqual(state, State.Empty);
            Assert.AreNotEqual(state.Hash, State.Empty.Hash);
            Assert.AreNotEqual(state.MachineToken, State.Empty.MachineToken);
        }

        [Test]
        public void TestAddingViewFormAnotherOne()
        {
            var ctx = new DefaultForestContext(new View.Factory(), null);
            ctx.ViewRegistry.Register<Inner.View>();
            ctx.ViewRegistry.Register<Outer.View>();

            var state1 = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.Shell, string.Empty, Outer.ViewName), State.Empty);

            Assert.AreNotEqual(state1, State.Empty);
            Assert.AreNotEqual(state1.Hash, State.Empty.Hash);
            Assert.AreNotEqual(state1.MachineToken, State.Empty.MachineToken);

            var state2 = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.Shell, string.Empty, Outer.ViewName), state1);

            Assert.AreNotEqual(state2, state1);
            Assert.AreNotEqual(state2.Hash, state1.Hash);
            Assert.AreEqual(state2.MachineToken, state1.MachineToken);
        }


        [Test]
        public void TestTraversal()
        {
            var ctx = new DefaultForestContext(new View.Factory(), null);
            ctx.ViewRegistry.Register<Inner.View>();
            ctx.ViewRegistry.Register<Outer.View>();

            var state1 = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.Shell, string.Empty, Outer.ViewName), State.Empty);

            Assert.AreNotEqual(state1, State.Empty);
            Assert.AreNotEqual(state1.Hash, State.Empty.Hash);
            Assert.AreNotEqual(state1.MachineToken, State.Empty.MachineToken);

            Renderer.traverse( new Visitor(), state1);
        }
    }
}
