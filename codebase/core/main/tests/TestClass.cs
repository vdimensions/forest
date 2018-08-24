﻿using System;

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
                var count2 = new Random().Next(1, 5);
                for (var i = 0; i < count2; i++)
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
            var key = HierarchyKey.NewKey(string.Empty, Inner.ViewName, HierarchyKey.Shell);
            var result = Engine.Update(ctx, ForestOperation.NewInstantiateView(key), State.Empty);

            Assert.AreNotEqual(result.State, State.Empty);
            Assert.AreNotEqual(result.State.Hash, State.Empty.Hash);
            Assert.AreNotEqual(result.State.MachineToken, State.Empty.MachineToken);
        }

        [Test]
        public void TestStateCompensation()
        {
            var key = HierarchyKey.NewKey(string.Empty, Outer.ViewName, HierarchyKey.Shell);
            var originalResult = Engine.Update(ctx, ForestOperation.NewInstantiateView(key), State.Empty);

            Assert.AreNotEqual(originalResult.State, State.Empty);
            Assert.AreNotEqual(originalResult.State.Hash, State.Empty.Hash);
            Assert.AreNotEqual(originalResult.State.MachineToken, State.Empty.MachineToken);

            var compensatedResult = Engine.ApplyChangeLog(ctx, State.Empty, originalResult.ChangeList);

           // Assert.AreEqual(originalResult.State, compensatedResult.State);
            Assert.IsTrue(originalResult.State.Equals(compensatedResult.State));
            Assert.AreEqual(originalResult.State.Hash, compensatedResult.State.Hash);
            Assert.AreEqual(originalResult.State.MachineToken, compensatedResult.State.MachineToken);
        }

        [Test]
        public void TestAddingViewFormAnotherOne()
        {
            var result1 = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.NewKey(string.Empty, Outer.ViewName, HierarchyKey.Shell)), State.Empty);

            Assert.AreNotEqual(result1.State, State.Empty);
            Assert.AreNotEqual(result1.State.Hash, State.Empty.Hash);
            Assert.AreNotEqual(result1.State.MachineToken, State.Empty.MachineToken);

            var result2 = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.NewKey(string.Empty, Outer.ViewName, HierarchyKey.Shell)), result1.State);

            Assert.AreNotEqual(result2.State, result1.State);
            Assert.AreNotEqual(result2.State.Hash, result1.State.Hash);
            Assert.AreEqual(result2.State.MachineToken, result1.State.MachineToken);
        }

        [Test]
        public void TestTraversal()
        {
            var result = Engine.Update(ctx, ForestOperation.NewInstantiateView(HierarchyKey.NewKey(string.Empty, Outer.ViewName, HierarchyKey.Shell)), State.Empty);

            Assert.AreNotEqual(result.State, State.Empty);
            Assert.AreNotEqual(result.State.Hash, State.Empty.Hash);
            Assert.AreNotEqual(result.State.MachineToken, State.Empty.MachineToken);

            Renderer.traverse( new PrintVisitor(), result.State);
        }
    }
}
