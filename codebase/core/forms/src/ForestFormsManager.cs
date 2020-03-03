// using System;
// using Axle.Modularity;
// using Axle.Verification;
// using Forest.ComponentModel;
// using Forest.Engine;
//
// namespace Forest.Forms
// {
//     public sealed class ForestFormsManager
//     {
//         [Module]
//         // TODO: IForestViewProvider interface is used to only to enable IForestEngine injection.
//         // TODO: Consider alternative ways to getting an `IForestEngine` instance
//         internal sealed class Module : IForestViewProvider 
//         {
//             private readonly ForestFormsManager _manager;
//
//             public Module(IForestEngine engine)
//             {
//                 _manager = new ForestFormsManager(engine);
//             }
//
//
//             [ModuleInit]
//             internal void Init(ModuleExporter exporter)
//             {
//                 exporter.Export(_manager);
//             }
//
//             public void RegisterViews(IViewRegistry registry) { }
//         }
//
//         private readonly IForestEngine _forestEngine;
//
//         private ForestFormsManager(IForestEngine forestEngine)
//         {
//             _forestEngine = forestEngine;
//         }
//
//         public void DelegateToEngine(Action<IForestEngine> operation) =>
//             operation.VerifyArgument(nameof(operation)).IsNotNull().Value.Invoke(_forestEngine);
//         public T DelegateToEngine<T>(Func<IForestEngine, T> operation) =>
//             operation.VerifyArgument(nameof(operation)).IsNotNull().Value.Invoke(_forestEngine);
//     }
// }