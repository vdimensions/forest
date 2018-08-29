using System;

using NUnit.Framework;


namespace Forest.Tests
{
    internal abstract class BaseEq
    {
        public override bool Equals(object obj) => true;
    }
    internal static class Inner
    {
        internal const string ViewName = "Inner";

        internal class ViewModel : BaseEq { }

        [View(ViewName)]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
            }
        }
    }

    internal static class Outer
    {
        internal const string ViewName = "Outer";
        internal class ViewModel : BaseEq { }

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
                var count2 = new Random().Next(0, 10);
                for (var i = 10; i <= count2; i++)
                {
                    region.ActivateView(Outer.ViewName);
                }
            }
        }
    }

    internal class PrintVisitor : IForestStateVisitor, IDomRenderer
    {
        public void BFS(HierarchyKey key, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine(">> BFS: {0} : {1}", index, key);
        }

        public void DFS(HierarchyKey key, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine("<< DFS: {0} : {1}", index, key);
        }

        public void Complete()
        {
            Console.WriteLine("--------------------------------------");
        }

        public DomNode ProcessNode(DomNode node)
        {
            Console.WriteLine("Node {0} {1}({2})", node.Index, node.Name, node.Key);
            return node;
        }
    }

    [TestFixture]
    public class TestClass
    {
        private IForestContext _ctx;

        [SetUp]
        public void SetUp()
        {
            _ctx = new DefaultForestContext(new View.Factory(), new NoopSecurityManager());
            _ctx.ViewRegistry.Register<Inner.View>();
            _ctx.ViewRegistry.Register<Outer.View>();
        }

        [Test]
        public void TestMethod()
        {
            var engine = new ForestEngine(_ctx);
            var result = engine.Update(
                e =>
                {
                    e.ActivateView<Inner.View>(Inner.ViewName);
                });

            Assert.AreNotEqual(result.State, State.Empty);
            Assert.AreNotEqual(result.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(result.State.MachineToken, State.Empty.MachineToken);

            Console.WriteLine("--------------------------------------");
            result.Render(new PrintVisitor());
        }

        [Test]
        public void TestStateTransferConsistency()
        {
            var engine1 = new ForestEngine(_ctx);
            var originalResult = engine1.Update(e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(originalResult.State, State.Empty);
            Assert.AreNotEqual(originalResult.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(originalResult.State.MachineToken, State.Empty.MachineToken);

            var engine2 = new ForestEngine(_ctx);
            var compensatedResult = engine2.Sync(originalResult.ChangeList);

            originalResult.Render(new PrintVisitor());
            compensatedResult.Render(new PrintVisitor());

            Assert.AreEqual(originalResult.State, compensatedResult.State);
            Assert.AreEqual(originalResult.State.Hash, compensatedResult.State.Hash);
            //Assert.AreEqual(originalResult.State.MachineToken, compensatedResult.State.MachineToken);
        }

        [Test]
        public void TestStateTransferConsistency100()
        {
            for (var i = 0; i < 1000; i++)
            {
                TestStateTransferConsistency();
                Console.WriteLine("======================================");
            }
        }

        [Test]
        public void TestAddingViewFormAnotherOne()
        {
            var engine1 = new ForestEngine(_ctx);
            var result1 = engine1.Update(e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(result1.State, State.Empty);
            Assert.AreNotEqual(result1.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(result1.State.MachineToken, State.Empty.MachineToken);

            var engine2 = new ForestEngine(_ctx);
            var result2 = engine2.Update( e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(result2.State, result1.State);
            Assert.AreNotEqual(result2.State.Hash, result1.State.Hash);
            //Assert.AreEqual(result2.State.MachineToken, result1.State.MachineToken);

            result1.Render(new PrintVisitor());
            result2.Render(new PrintVisitor());
        }

    }
}
