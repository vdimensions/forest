﻿using System.Diagnostics.CodeAnalysis;
using Forest.Globalization;
using Forest.Navigation;

namespace Forest.UI.Navigation
{
    public static partial class NavigationMenu
    {
        internal static class Item
        {
            private const string Name = "NavigationMenuItem";

            [View(Name)]
            internal class View : LogicalView<NavigationNode>
            {
                protected View(NavigationNode model) : base(model) { }

                protected override string ResourceBundle => Model != null ? $"{Name}.{Model.Path.Replace("/", ".")}" : null;
            }
        }
        
        internal static class NavigableItem
        {
            private const string Name = "NavigationMenuNavigableItem";
            
            private static class Commands
            {
                internal const string Navigate = "Navigate";
            }
            
            [View(Name)]
            internal sealed class View : NavigationMenu.Item.View
            {
                public View(NavigationNode model) : base(model) { }

                [Command(Commands.Navigate)]
                [SuppressMessage("ReSharper", "UnusedMember.Global")]
                internal Location Navigate() => Location.Create(Model.Path);
            }
        }
    }
}