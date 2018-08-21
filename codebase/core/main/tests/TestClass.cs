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

        [View(ViewName)]
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
            var ctx = new DefaultForestContext(new DefaultViewRegistry(new View.Factory()));
            ctx.ViewRegistry.Register<My.View>();
            var state = Forest.Engine.Update(ctx, ForestOperation.NewInstantiateView(Identifier.Shell, string.Empty, My.ViewName), State.Empty);

            Assert.AreNotEqual(state, State.Empty);
        }
    }
}
