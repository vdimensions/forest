using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;


namespace Forest.Forms.Controls
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class Select
    {
        public static class Option
        {
            public static class Regions
            {
                internal const string Item = "Item";
            }

            public static class Commands
            {
                internal const string Select = "Select";
            }

            public class Model<TItemModel>
            {
                [DebuggerBrowsable(DebuggerBrowsableState.Never)]
                private readonly TItemModel _itemModel;

                public Model(TItemModel itemModel, bool selected)
                {
                    _itemModel = itemModel;
                    Selected = selected;
                }

                public TItemModel ItemModel => _itemModel;
                public bool Selected { get; }
            }

            public class View<TView, TModel> : LogicalView<Model<TModel>> where TView : IView<TModel>
            {
                public View(Model<TModel> model) : base(model) { }

                [Command(Commands.Select)]
                internal void Select()
                {
                    if (Model.Selected)
                    {
                        return;
                    }
                    Selected?.Invoke(this);
                }

                public override void Load()
                {
                    ItemView = FindRegion(Regions.Item).Clear().ActivateView<TView, TModel>(Model.ItemModel);
                }

                public override void Dispose(bool disposing)
                {
                    if (disposing)
                    {
                        Selected = null;
                    }
                    base.Dispose(disposing);
                }

                internal event Action<View<TView, TModel>> Selected;
                internal TView ItemView { get; set; }
            }
        }
        
        public class View<TItemView, TItemModel> : Repeater.View<Option.View<TItemView, TItemModel>, Option.Model<TItemModel>>
            where TItemView : IView<TItemModel>
        {
            private IEnumerable<Option.View<TItemView, TItemModel>> _optionViews;
            private readonly IEqualityComparer<TItemModel> _itemComparer;
            private readonly bool _allowMultipleSelection = false;

            public View(IEqualityComparer<TItemModel> itemComparer, bool allowMultipleSelection)
            {
                _itemComparer = itemComparer;
                _allowMultipleSelection = allowMultipleSelection;
            }
            public View(bool allowMultipleSelection) : this(EqualityComparer<TItemModel>.Default, allowMultipleSelection) { }
            public View() : this(EqualityComparer<TItemModel>.Default, false) { }

            public IEnumerable<TItemView> Populate(TItemModel[] items) => Populate(items, 0);
            public IEnumerable<TItemView> Populate(TItemModel[] items, TItemModel selectedItem)
            {
                var ix = items
                    .Select((x, i) => new { x, i })
                    .SingleOrDefault(x => _itemComparer.Equals(x.x, selectedItem))
                    ?.i ?? 0;
                return Populate(items, ix);
            }

            public IEnumerable<TItemView> Populate(TItemModel[] items, int selectedIndex)
            {
                var result = new LinkedList<TItemView>();
                _optionViews = Populate(items.Select((x, i) => new Option.Model<TItemModel>(x, i == selectedIndex)));
                foreach (var view in _optionViews)
                {
                    view.Selected += View_Selected;
                    result.AddLast(view.ItemView);
                }
                return result;
            }

            private void View_Selected(Option.View<TItemView, TItemModel> selectedOption)
            {
                foreach (var optionView in _optionViews)
                {
                    var isTheCurrentItem = _itemComparer.Equals(optionView.Model.ItemModel, selectedOption.Model.ItemModel);
                    var selected = isTheCurrentItem || (_allowMultipleSelection && optionView.Model.Selected);
                    optionView.UpdateModel(m => new Option.Model<TItemModel>(m.ItemModel, selected));
                }
                SelectionChanged?.Invoke(selectedOption.ItemView.Model);
            }

            public override void Dispose(bool disposing)
            {
                SelectionChanged = null;
                base.Dispose(disposing);
            }

            public event Action<TItemModel> SelectionChanged;
        }
    }
}