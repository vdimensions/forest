using System;
using System.IO;

using Forest.Templates;
using Forest.Templates.Raw;
using Forest.Templates.Xml;

using NUnit.Framework;


namespace Forest.Tests
{
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
        private ITemplateProvider _templateProvider;

        [SetUp]
        public void SetUp()
        {
            _templateProvider = new TestTemplateProvider(new XmlTemplateParser());
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
        public void ParseSlave1Template()
        {
            var template = Raw.loadTemplate(_templateProvider, "Slave1");
            Assert.IsNotNull(template);
            Assert.AreEqual("Slave1", template.name);
            Assert.IsNotEmpty(template.contents);
            Assert.AreEqual(template.contents.Length, 3);

            var compiled = TemplateCompiler.Compile(template);
            Assert.IsNotNull(compiled);
        }
    }
}
