using System;

using Forest.Rendering;

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

    internal class PrintVisitor : IStateVisitor
    {
        public void BFS(HierarchyKey key, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine(">> BFS: {0} : {1}", index, key);
        }

        public void DFS(HierarchyKey key, int index, object viewModel, IViewDescriptor descriptor)
        {
            Console.WriteLine("<< DFS: {0} : {1}", index, key);
        }

        public void Done()
        {
            Console.WriteLine("--------------------------------------");
        }
    }

    [TestFixture]
    public class TestClass
    {
        private IForestContext ctx;

        [SetUp]
        public void SetUp()
        {
            ctx = new DefaultForestContext(new View.Factory(), null);
            ctx.ViewRegistry.Register<Inner.View>();
            ctx.ViewRegistry.Register<Outer.View>();
        }

        [Test]
        public void TestMethod()
        {
            var result = State.Empty.Update(
                ctx,
                e =>
                {
                    e.ActivateView<Inner.View>(Inner.ViewName);
                });

            Assert.AreNotEqual(result.State, State.Empty);
            Assert.AreNotEqual(result.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(result.State.MachineToken, State.Empty.MachineToken);

            Console.WriteLine("--------------------------------------");
            Renderer.traverse(new PrintVisitor(), result.State);
        }

        [Test]
        public void TestStateTransferConsistency()
        {
            var originalResult = State.Empty.Update(ctx, e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(originalResult.State, State.Empty);
            Assert.AreNotEqual(originalResult.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(originalResult.State.MachineToken, State.Empty.MachineToken);

            var compensatedResult = State.Empty.Sync(ctx, originalResult.ChangeList);

            Renderer.traverse(new PrintVisitor(), originalResult.State);
            Renderer.traverse(new PrintVisitor(), compensatedResult.State);

            Assert.AreEqual(originalResult.State, compensatedResult.State);
            //Assert.IsTrue(originalResult.State.Equals(compensatedResult.State));
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
            var result1 = State.Empty.Update(ctx, e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(result1.State, State.Empty);
            Assert.AreNotEqual(result1.State.Hash, State.Empty.Hash);
            //Assert.AreNotEqual(result1.State.MachineToken, State.Empty.MachineToken);

            var result2 = result1.State.Update(ctx, e => e.ActivateView<Outer.View>(Outer.ViewName));

            Assert.AreNotEqual(result2.State, result1.State);
            Assert.AreNotEqual(result2.State.Hash, result1.State.Hash);
            //Assert.AreEqual(result2.State.MachineToken, result1.State.MachineToken);

            Renderer.traverse(new PrintVisitor(), result1.State);
            Renderer.traverse(new PrintVisitor(), result2.State);
        }

    }
}
