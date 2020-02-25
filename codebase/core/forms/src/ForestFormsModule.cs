using Axle.Modularity;
using Forest.Engine;
using Forest.ComponentModel;
using Forest.Forms.Controls;
using Forest.Forms.Controls.Dialogs;
using System;
using Axle.Verification;

namespace Forest.Forms
{
    [Requires(typeof(ForestFormsManager.Module))]
    internal interface IForestFormsManagerProvider
    {
    }

    public sealed class ForestFormsManager
    {
        [Module]
        // TODO: IForestViewProvider interface is used to enable IForestEngine injection. Consider alternative ways 
        internal sealed class Module : IForestViewProvider 
        {
            private readonly ForestFormsManager _manager;

            public Module(IForestEngine engine)
            {
                _manager = new ForestFormsManager(engine);
            }


            [ModuleInit]
            internal void Init(ModuleExporter exporter)
            {
                exporter.Export(_manager);
            }

            public void RegisterViews(IViewRegistry registry) { }
        }

        private readonly IForestEngine _forestEngine;

        internal ForestFormsManager(IForestEngine forestEngine)
        {
            _forestEngine = forestEngine;
        }

        public void DelegateToEngine(Action<IForestEngine> operation) =>
            operation.VerifyArgument(nameof(operation)).IsNotNull().Value.Invoke(_forestEngine);
        public T DelegateToEngine<T>(Func<IForestEngine, T> operation) =>
            operation.VerifyArgument(nameof(operation)).IsNotNull().Value.Invoke(_forestEngine);
    }

    [Module]
    [Requires(typeof(ForestDialogsModule))]
    internal sealed class ForestFormsModule : IForestViewProvider, IForestFormsManagerProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<TabStrip.Tab.View>()
                .Register<TabStrip.View>();
        }
    }
}
