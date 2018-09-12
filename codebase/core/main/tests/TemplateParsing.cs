using System;
using System.IO;

using Forest.Reflection;
using Forest.Security;
using Forest.Templates;
using Forest.Templates.Raw;
using Forest.Templates.Xml;

using NUnit.Framework;


namespace Forest.Tests
{
    public static class Navigation
    {
        internal class ViewModel { }

        [View("Navigation")]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
            }
        }
    }
    public static class SimpleFooter
    {
        internal class ViewModel { }

        [View("SimpleFooter")]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
            }
        }
    }
    public static class Concrete
    {
        internal class ViewModel { }

        [View("Concrete")]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
            }
        }

    }
    public static class SomeView
    {
        internal class ViewModel { }

        [View("SomeView")]
        internal class View : AbstractView<ViewModel>
        {
            public override void Load()
            {
            }
        }
    }

    [TestFixture]
    public class TemplateParsing
    {
        internal sealed class TestTemplateProvider : ITemplateProvider
        {
            internal static Stream OpenTemplate(string name)
            {
                var fullName = string.Format("Templates/{0}.xml", name);
                var searchPath = AppDomain.CurrentDomain.RelativeSearchPath ?? AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(searchPath, fullName);
                return File.OpenRead(path);
            }

            private AbstractTemplateParser _parser;


            public TestTemplateProvider(AbstractTemplateParser parser)
            {
                _parser = parser;
            }

            public Template Load(string name)
            {
                using (var stream = OpenTemplate(name))
                {
                    return _parser.Parse(name, stream);
                }
            }
        }
        
        private XmlTemplateParser _parser;
        private IForestContext _ctx;
        private ITemplateProvider _templateProvider;

        [SetUp]
        public void SetUp()
        {
            var f = new View.Factory();
            var rp = new DefaultReflectionProvider();
            _templateProvider = new TestTemplateProvider(new XmlTemplateParser());
            _ctx = new DefaultForestContext(f, rp, new NoopSecurityManager(), _templateProvider);
            _ctx.ViewRegistry.Register<Navigation.View>();
            _ctx.ViewRegistry.Register<Concrete.View>();
            _ctx.ViewRegistry.Register<SomeView.View>();
            _ctx.ViewRegistry.Register<SimpleFooter.View>();
        }

        [Test]
        public void CanReadTemplateFile()
        {
            using (var stream = TestTemplateProvider.OpenTemplate("Master"))
            {
                Assert.IsNotNull(stream, "Unable to load template Master");
            }
        }

        [Test]
        public void ParseMasterTemplate()
        {
            var template = Raw.loadTemplate(_templateProvider, "Master");
            Assert.IsNotNull(template);
            Assert.AreEqual("Master", template.name);
            Assert.IsNotEmpty(template.contents);
        }

        [Test]
        public void ParseConcreteTemplate()
        {
            var template = Raw.loadTemplate(_templateProvider, "Concrete");
            Assert.IsNotNull(template);
            Assert.AreEqual("Concrete", template.name);
            Assert.IsNotEmpty(template.contents);
            Assert.AreEqual(template.contents.Length, 3);

            var compiled = TemplateCompiler.Compile(template);
            Assert.IsNotNull(compiled);
        }

        [Test]
        public void LoadConcreteTemplate()
        {
            var engine = new ForestEngine(_ctx);
            var r = engine.LoadTemplate("Concrete");
        }
    }
}
