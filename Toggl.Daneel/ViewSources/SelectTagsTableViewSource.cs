using System;
using Foundation;
using MvvmCross.Binding.iOS.Views;
using Toggl.Daneel.Views;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class SelectTagsTableViewSource : MvxTableViewSource
    {
        private const string cellIdentifier = nameof(SelectableTagViewCell);

        public SelectTagsTableViewSource(UITableView tableView) 
            : base(tableView)
        {
            tableView.RegisterNibForCellReuse(SelectableTagViewCell.Nib, cellIdentifier);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 48;

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(cellIdentifier, indexPath);
    }
}
