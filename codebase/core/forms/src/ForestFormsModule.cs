﻿using Axle.Modularity;
using Forest.Forms.Controls;
using Forest.Forms.Controls.Dialogs;


namespace Forest.Forms
{
    [Module]
    [RequiresForest]
    [Requires(typeof(ForestDialogsModule))]
    internal sealed class ForestFormsModule : IForestViewProvider
    {
        public void RegisterViews(IViewRegistry registry)
        {
            registry
                .Register<TabStrip.Tab.View>()
                .Register<TabStrip.View>();
        }
    }
}
