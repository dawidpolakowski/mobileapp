using System;
using System.Linq;
using CoreGraphics;
using Foundation;
using MvvmCross.Binding.ExtensionMethods;
using MvvmCross.Binding.iOS.Views;
using MvvmCross.Core.ViewModels;
using Toggl.Daneel.Views;
using Toggl.Foundation;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class SuggestionsTableViewSource : MvxTableViewSource
    {
        private const int rowHeight = 64;
        private const int headerHeight = 45;
        private const string cellIdentifier = nameof(SuggestionsViewCell);
        private const string emptyCellIdentifier = nameof(SuggestionsEmptyViewCell);

        private int maxNumberOfSuggestions;

        public IMvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        public SuggestionsTableViewSource(UITableView tableView)
            : base(tableView)
        {
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.RegisterNibForCellReuse(SuggestionsViewCell.Nib, cellIdentifier);
            tableView.RegisterNibForCellReuse(SuggestionsEmptyViewCell.Nib, emptyCellIdentifier);
        }

        public override void ReloadTableData()
        {
            maxNumberOfSuggestions = (int)(TableView.Bounds.Height - headerHeight) / rowHeight;
            base.ReloadTableData();
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            var totalWidth = UIScreen.MainScreen.Bounds.Width;
            var header = new UIView(new CGRect(0, 0, totalWidth, 45))
            {
                new UILabel(new CGRect(16, 16, totalWidth - 16, 19))
                {
                    Font = UIFont.SystemFontOfSize(16, UIFontWeight.Medium),
                    Text = Resources.SuggestionsHeader
                }
            };
            header.BackgroundColor = UIColor.White;
            return header;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = GetItemAt(indexPath);
            var cell = GetOrCreateCellFor(tableView, indexPath, item);
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            if (item != null && cell is IMvxBindable bindable)
                bindable.DataContext = item;

            if (cell is SuggestionsViewCell suggestionCell)
                suggestionCell.StartTimeEntryCommand = StartTimeEntryCommand;

            return cell;
        }

        public override nint NumberOfSections(UITableView tableView) => 1;

        public override nint RowsInSection(UITableView tableview, nint section)
            => ItemsSource.Count().Clamp(0, maxNumberOfSuggestions);

        protected override object GetItemAt(NSIndexPath indexPath)
        {
            if (ItemsSource.Count() == 0) return null;

            return ItemsSource.ElementAt((int)indexPath.Item);
        }

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(item == null ? emptyCellIdentifier : cellIdentifier, indexPath);

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public override nfloat GetHeightForHeader(UITableView tableView, nint section) => headerHeight;
    }
}
