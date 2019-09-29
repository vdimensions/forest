//using System.Globalization;
//
//using Axle;
//using Axle.DependencyInjection;
//using Axle.Resources;
//
//using Forest.Templates.Raw;
//using Forest.UI;
//using NUnit.Framework;


namespace Forest.Core.Tests
{
    //public static class Navigation
    //{
    //    internal class ViewModel { }
    //
    //    [View("Navigation")]
    //    internal class View : LogicalView<ViewModel>
    //    {
    //        public View(ViewModel vm) : base(vm)
    //        {
    //        }
    //
    //        public override void Load()
    //        {
    //        }
    //    }
    //}
    //public static class SimpleFooter
    //{
    //    internal class ViewModel { }
    //
    //    [View("SimpleFooter")]
    //    internal class View : LogicalView<ViewModel>
    //    {
    //        public View(ViewModel vm) : base(vm)
    //        {
    //        }
    //
    //        public override void Load()
    //        {
    //        }
    //    }
    //}
    //public static class Concrete
    //{
    //    internal class ViewModel { }
    //
    //    [View("Concrete")]
    //    internal class View : LogicalView<ViewModel>
    //    {
    //        public View(ViewModel vm) : base(vm)
    //        {
    //        }
    //
    //        public override void Load()
    //        {
    //        }
    //    }
    //
    //}
    //public static class SomeView
    //{
    //    internal class ViewModel { }
    //
    //    [View("SomeView")]
    //    internal class View : LogicalView<ViewModel>
    //    {
    //        public View(ViewModel vm) : base(vm)
    //        {
    //        }
    //
    //        public override void Load()
    //        {
    //        }
    //    }
    //}
    //
    //[TestFixture]
    //public class TemplateParsing
    //{
    //    private IForestEngine _forest;
    //    private IViewRegistry _vr;
    //    private ResourceManager _resourceManager;
    //    private CultureInfo _invariantCulture;
    //
    //    [SetUp]
    //    public void SetUp()
    //    {
    //        var app = Application.Build()
    //            .LoadForest()
    //            .Run();
    //        _forest = app.Container.Resolve<IForestEngine>();
    //        _vr = app.Container.Resolve<IViewRegistry>();
    //        _resourceManager = app.Container.Resolve<ResourceManager>();
    //        _invariantCulture = CultureInfo.InvariantCulture;
    //
    //        _vr.Register<Navigation.View>();
    //        _vr.Register<Concrete.View>();
    //        _vr.Register<SomeView.View>();
    //        _vr.Register<SimpleFooter.View>();
    //    }
    //
    //    [Test]
    //    public void ParseMasterTemplate()
    //    {
    //        var templateRes = _resourceManager.Load("ForestTemplates", "Master", _invariantCulture);
    //        Assert.IsTrue(templateRes.HasValue);
    //
    //        var template = templateRes.Value.Resolve<Template>();
    //        Assert.IsNotNull(template);
    //
    //        var templateDefinition = GetDefinition(template);
    //        Assert.IsNotNull(templateDefinition);
    //
    //        Assert.IsFalse(template.IsMastered);
    //
    //        Assert.AreEqual("Master", templateDefinition.Name);
    //        Assert.IsNotEmpty(templateDefinition.Contents);
    //    }
    //
    //    private TemplateDefinition GetDefinition(Template template)
    //    {
    //        return template.item;
    //    }
    //
    //    [Test]
    //    public void ParseConcreteTemplate()
    //    {
    //        //var template = Raw.loadTemplate(_templateProvider, "Concrete");
    //        var templateRes = _resourceManager.Load("ForestTemplates", "Concrete", _invariantCulture);
    //        Assert.IsTrue(templateRes.HasValue);
    //
    //        var template = templateRes.Value.Resolve<Template>();
    //        Assert.IsNotNull(template);
    //
    //        //var templateDefinition = GetDefinition(template);
    //        //Assert.IsNotNull(templateDefinition);
    //        //
    //        //Assert.AreEqual("Concrete", templateDefinition.Name);
    //        //Assert.IsNotEmpty(templateDefinition.Contents);
    //        //Assert.AreEqual(templateDefinition.Contents.Length, 3);
    //        //
    //        //var compiled = TemplateCompiler.Compile(templateDefinition);
    //        //Assert.IsNotNull(compiled);
    //    }
    //}
}
