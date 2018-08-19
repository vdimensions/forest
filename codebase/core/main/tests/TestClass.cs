using System;
using System.Collections.Generic;

using Forest;

using NUnit.Framework;


namespace Forest.Tests
{
    static class My
    {
        internal const string ViewName = "MyView";
        internal class ViewModel { }

        [View(ViewName, AutowireCommands = false)]
        internal class View : Forest.AbstractView<ViewModel>
        {
            public override void ResumeState(ViewModel vm)
            {
                throw new NotImplementedException();
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
            //IDictionary<string, object> rawTemplateStructureFromJson = new Dictionary<string, object>();
            //
            //var x = Add(Get(Add(Get(Add(rawTemplateStructureFromJson, "rootView"), "rootView"), "contentRegion"), "contentRegion"), "MyView");
            //Add(Get(Add(x, "view2"), "view2"), "emptyRegion");
            //
            var ctx = new DefaultForestContext(new DefaultViewRegistry(new View.Factory()));
            ctx.ViewRegistry.Register<My.View>();
            var state = Forest.Engine.Update(ctx, ForestOperation.NewInstantiateView(IdentifierModule.Shell, string.Empty, My.ViewName), State.Empty);

            //var index = Forest.Engine.CreateIndex(ctx, rawTemplateStructureFromJson);
            //
            //Console.WriteLine("dom index contains {0} root nodes", index.Count);
            //foreach (var path in index.Paths)
            //foreach (var domNode in index[path].Value)
            //{
            //    Console.WriteLine("  +-[{0}]", domNode.Path.ToString());
            //}
            //
            //var execIdnex = Forest.Engine.Execute(ctx, index);
        }
    }
}
