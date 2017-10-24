﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.DataSources;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectTagsViewModel : MvxViewModel<(long[] tagIds, long workspaceId), long[]>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ITogglDataSource dataSource;
        private readonly Subject<string> textSubject = new Subject<string>();
        private readonly HashSet<long> selectedTagIds = new HashSet<long>();

        private long[] defaultResult;
        private long workspaceId;

        public string Text { get; set; } = "";

        public MvxObservableCollection<SelectableTagViewModel> Tags { get; }
            = new MvxObservableCollection<SelectableTagViewModel>();

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand SaveCommand { get; }

        public IMvxCommand<SelectableTagViewModel> SelectTagCommand { get; }

        public SelectTagsViewModel(ITogglDataSource dataSource, IMvxNavigationService navigationService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));

            this.dataSource = dataSource;
            this.navigationService = navigationService;

            CloseCommand = new MvxAsyncCommand(close);
            SaveCommand = new MvxAsyncCommand(save);
            SelectTagCommand = new MvxCommand<SelectableTagViewModel>(selectTag);
        }
        
        public override void Prepare((long[] tagIds, long workspaceId) parameter)
        {
            workspaceId = parameter.workspaceId;
            defaultResult = parameter.tagIds;
            foreach (var tagId in parameter.tagIds)
                selectedTagIds.Add(tagId);
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            textSubject.AsObservable()
                       .StartWith(Text)
                       .SelectMany(text => dataSource.AutocompleteProvider.Query(text, AutocompleteSuggestionType.Tags))
                       .Select(suggestions => suggestions.Cast<TagSuggestion>())
                       .Select(suggestions => suggestions.Where(s => s.WorkspaceId == workspaceId))
                       .Subscribe(onTags);
        }

        private void OnTextChanged()
        {
            textSubject.OnNext(Text);
        }

        private void onTags(IEnumerable<TagSuggestion> tags)
        {
            Tags.Clear();

            Tags.AddRange(tags.Select(createSelectableTag));
        }

        private SelectableTagViewModel createSelectableTag(TagSuggestion tagSuggestion)
            => new SelectableTagViewModel(tagSuggestion, selectedTagIds.Contains(tagSuggestion.TagId));

        private Task close()
            => navigationService.Close(this, defaultResult);

        private Task save() => navigationService.Close(this, selectedTagIds.ToArray());

        private void selectTag(SelectableTagViewModel tag)
        {
            tag.Selected = !tag.Selected;

            if (tag.Selected)
                selectedTagIds.Add(tag.Id);
            else
                selectedTagIds.Remove(tag.Id);
        }
    }
}
