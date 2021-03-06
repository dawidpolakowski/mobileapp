﻿using System;
using Foundation;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Color.iOS;
using Toggl.Daneel.Views;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.Helper;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class StartTimeEntryTableViewSource : GroupedCollectionTableViewSource<AutocompleteSuggestion>
    {
        private const string tagCellIdentifier = nameof(TagSuggestionViewCell);
        private const string taskCellIdentifier = nameof(TaskSuggestionViewCell);
        private const string headerCellIdentifier = nameof(WorkspaceHeaderViewCell);
        private const string timeEntryCellIdentifier = nameof(StartTimeEntryViewCell);
        private const string projectCellIdentifier = nameof(ProjectSuggestionViewCell);
        private const string emptySuggestionIdentifier = nameof(StartTimeEntryEmptyViewCell);

        public bool UseGrouping { get; set; }

        public IMvxCommand<ProjectSuggestion> ToggleTasksCommand { get; set; }

        public StartTimeEntryTableViewSource(UITableView tableView)
            : base(tableView, headerCellIdentifier, "")
        {
            tableView.TableFooterView = new UIView();
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tableView.SeparatorColor = Color.StartTimeEntry.SeparatorColor.ToNativeColor();
            tableView.RegisterNibForCellReuse(TagSuggestionViewCell.Nib, tagCellIdentifier);
            tableView.RegisterNibForCellReuse(TaskSuggestionViewCell.Nib, taskCellIdentifier);
            tableView.RegisterNibForCellReuse(StartTimeEntryViewCell.Nib, timeEntryCellIdentifier);
            tableView.RegisterNibForCellReuse(ProjectSuggestionViewCell.Nib, projectCellIdentifier);
            tableView.RegisterNibForCellReuse(StartTimeEntryEmptyViewCell.Nib, emptySuggestionIdentifier);
            tableView.RegisterNibForHeaderFooterViewReuse(WorkspaceHeaderViewCell.Nib, headerCellIdentifier);
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {
            if (!UseGrouping) return null;

            return base.GetViewForHeader(tableView, section);
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = base.GetCell(tableView, indexPath);
            cell.LayoutMargins = UIEdgeInsets.Zero;
            cell.SeparatorInset = UIEdgeInsets.Zero;
            cell.PreservesSuperviewLayoutMargins = false;

            if (cell is ProjectSuggestionViewCell projectCell)
            {
                projectCell.ToggleTasksCommand = ToggleTasksCommand;
                
                var previousItemPath = NSIndexPath.FromItemSection(indexPath.Item - 1, indexPath.Section);
                var previous = GetItemAt(previousItemPath);
                var previousIsTask = previous is TaskSuggestion;
                projectCell.TopSeparatorHidden = !previousIsTask;
            }

            return cell;
        }

        protected override UITableViewHeaderFooterView GetOrCreateHeaderViewFor(UITableView tableView)
            => tableView.DequeueReusableHeaderFooterView(headerCellIdentifier);

        protected override UITableViewCell GetOrCreateCellFor(UITableView tableView, NSIndexPath indexPath, object item)
            => tableView.DequeueReusableCell(getIdentifier(item), indexPath);

        public override nfloat GetHeightForHeader(UITableView tableView, nint section) 
            => UseGrouping ? 40 : 0;

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => 48;

        private string getIdentifier(object item)
        {
            if (item is ProjectSuggestion)
                return projectCellIdentifier;

            if (item is QuerySymbolSuggestion)
                return emptySuggestionIdentifier;

            if (item is TagSuggestion)
                return tagCellIdentifier;
            
            if (item is TaskSuggestion)
                return taskCellIdentifier;

            return timeEntryCellIdentifier;
        }
    }
}
