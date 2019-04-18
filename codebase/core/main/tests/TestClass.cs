using System;

using Forest.Reflection;
using Forest.Security;
using Forest.Templates.Xml;

using NUnit.Framework;


namespace Forest.Tests
{
    //internal abstract class BaseEq
    //{
    //    public override bool Equals(object obj) => true;
    //}
    //internal static class Inner
    //{
    //    internal const string ViewName = "Inner";
    //
    //    internal class ViewModel : BaseEq { }
    //
    //    [View(ViewName)]
    //    internal class View : AbstractView<ViewModel>
    //    {
    //        public View(ViewModel model) : base(model) {}
    //
    //        public override void Load()
    //        {
    //        }
    //
    //        [Command("command")]
    //        internal void CommandExample(string arg) { }
    //
    //        [Command("commandNoArgs")]
    //        internal void CommandExample2() { }
    //    }
    //}
    //
    //internal static class Outer
    //{
    //    internal const string ViewName = "Outer";
    //    internal class ViewModel : BaseEq { }
    //
    //    [View(ViewName)]
    //    internal class View : AbstractView<ViewModel>
    //    {
    //        public override void Load()
    //        {
    //            var region = FindRegion("SomeRegion").Clear();
    //            var count = new Random().Next(1, 10);
    //            for (var i = 0; i < count; i++)
    //            {
    //                var innerView = (Inner.View) region.ActivateView(Inner.ViewName, new Inner.ViewModel());
    //            }
    //            var count2 = new Random().Next(0, 10);
    //            for (var i = 10; i <= count2; i++)
    //            {
    //                region.ActivateView(Outer.ViewName);
    //            }
    //        }
    //    }
    //}
    //
    //internal class PrintProcessor : IDomProcessor
    //{
    //    public void Complete()
    //    {
    //        Console.WriteLine("--------------------------------------");
    //    }
    //
    //    public DomNode ProcessNode(DomNode node)
    //    {
    //        Console.WriteLine("Node {0} {1}({2})", node.Index, node.Name, node.Hash);
    //        return node;
    //    }
    //}
    //
    //[TestFixture]
    //public class TestClass
    //{
    //    private IForestContext _ctx;
    //
    //    [SetUp]
    //    public void SetUp()
    //    {
    //        var templateProvider = new TemplateParsing.TestTemplateProvider(new XmlTemplateParser());
    //        _ctx = new DefaultForestContext(new View.Factory(), new DefaultReflectionProvider(), new NoopSecurityManager(), templateProvider);
    //        _ctx.ViewRegistry.Register<Inner.View>();
    //        _ctx.ViewRegistry.Register<Outer.View>();
    //    }
    //
    //    [Test]
    //    public void TestMethod()
    //    {
    //        var engine = new ForestEngine(_ctx);
    //        var result = engine.Update(
    //            e =>
    //            {
    //                e.ActivateView<Inner.View, Inner.ViewModel>(Inner.ViewName, new Inner.ViewModel());
    //            });
    //
    //        Assert.AreNotEqual(result.State, State.Empty);
    //        Assert.AreNotEqual(result.State.Hash, State.Empty.Hash);
    //        //Assert.AreNotEqual(result.State.MachineToken, State.Empty.MachineToken);
    //
    //        Console.WriteLine("--------------------------------------");
    //        result.Render(new PrintProcessor());
    //    }
    //
    //    [Test]
    //    public void TestStateTransferConsistency()
    //    {
    //        var engine1 = new ForestEngine(_ctx);
    //        var originalResult = engine1.Update(e => e.ActivateView<Outer.View>(Outer.ViewName));
    //
    //        Assert.AreNotEqual(originalResult.State, State.Empty);
    //        Assert.AreNotEqual(originalResult.State.Hash, State.Empty.Hash);
    //        //Assert.AreNotEqual(originalResult.State.MachineToken, State.Empty.MachineToken);
    //
    //        var engine2 = new ForestEngine(_ctx);
    //        var compensatedResult = engine2.Sync(originalResult.ChangeList);
    //
    //        originalResult.Render(new PrintProcessor());
    //        compensatedResult.Render(new PrintProcessor());
    //
    //        Assert.AreEqual(originalResult.State, compensatedResult.State);
    //        Assert.AreEqual(originalResult.State.Hash, compensatedResult.State.Hash);
    //        //Assert.AreEqual(originalResult.State.MachineToken, compensatedResult.State.MachineToken);
    //    }
    //
    //    [Test]
    //    public void TestStateTransferConsistency1000()
    //    {
    //        for (var i = 0; i < 1000; i++)
    //        {
    //            TestStateTransferConsistency();
    //            Console.WriteLine("======================================");
    //        }
    //    }
    //
    //    [Test]
    //    public void TestAddingViewFormAnotherOne()
    //    {
    //        var engine1 = new ForestEngine(_ctx);
    //        var result1 = engine1.Update(e => e.ActivateView<Outer.View>(Outer.ViewName));
    //
    //        Assert.AreNotEqual(result1.State, State.Empty);
    //        Assert.AreNotEqual(result1.State.Hash, State.Empty.Hash);
    //        //Assert.AreNotEqual(result1.State.MachineToken, State.Empty.MachineToken);
    //
    //        var engine2 = new ForestEngine(_ctx);
    //        var result2 = engine2.Update( e => e.ActivateView<Outer.View>(Outer.ViewName));
    //
    //        Assert.AreNotEqual(result2.State, result1.State);
    //        Assert.AreNotEqual(result2.State.Hash, result1.State.Hash);
    //        //Assert.AreEqual(result2.State.MachineToken, result1.State.MachineToken);
    //
    //        result1.Render(new PrintProcessor());
    //        result2.Render(new PrintProcessor());
    //    }
    //
    //}
}
