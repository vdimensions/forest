using System;
using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;


namespace Forest.Tests
{
    [TestFixture]
    public class TestClass
    {
        class MyViewModel { }

        [View("MyView", AutowireCommands = true)]
        class MyView : AbstractView<MyViewModel> { }

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
            IDictionary<string, object> rawTemplateStructureFromJson = new Dictionary<string, object>();

            var x = Add(Get(Add(Get(Add(rawTemplateStructureFromJson, "rootView"), "rootView"), "contentRegion"), "contentRegion"), "MyView");
            Add(Get(Add(x, "view2"), "view2"), "emptyRegion");

            var ctx = new DefaultForestContext(new DefaultViewRegistry(new DefaultContainer()));
            var engine = new DefaultForestEngine();
            var index = engine.CreateIndex(ctx, rawTemplateStructureFromJson);

            Console.WriteLine("dom index contains {0} root nodes", index.Count);
            foreach (var path in index.Paths)
            foreach (var domNode in index[path].Value)
            {
                Console.WriteLine("  +-[{0}]", domNode.Path.ToString());
            }

            var execIdnex = engine.Execute(ctx, index);
        }
    }
}
