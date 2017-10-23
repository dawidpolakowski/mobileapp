using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.Exceptions;
using Toggl.Foundation.Suggestions;
using Toggl.Multivac;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SuggestionsViewModel : MvxViewModel
    {
        private readonly ITogglDataSource dataSource;
        private readonly ISuggestionProviderContainer suggestionProviders;
        private readonly ITimeService timeService;

        private IDisposable emptyDatabaseDisposable;

        public MvxObservableCollection<Suggestion> Suggestions { get; }
            = new MvxObservableCollection<Suggestion>();

        [DependsOn(nameof(Suggestions))]
        public bool IsEmpty => !Suggestions.Any();

        public IMvxAsyncCommand<Suggestion> StartTimeEntryCommand { get; set; }

        public SuggestionsViewModel(
            ITogglDataSource dataSource,
            ISuggestionProviderContainer suggestionProviders,
            ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(suggestionProviders, nameof(suggestionProviders));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.dataSource = dataSource;
            this.suggestionProviders = suggestionProviders;
            this.timeService = timeService;

            StartTimeEntryCommand = new MvxAsyncCommand<Suggestion>(startTimeEntry);
        }

        public async override Task Initialize()
        {
            await base.Initialize();

            emptyDatabaseDisposable = dataSource.TimeEntries.IsEmpty
                .DistinctUntilChanged()
                .Subscribe(fetchSuggestions);
        }

        private void fetchSuggestions(bool databaseIsEmpty)
        {
            Suggestions.Clear();

            if (databaseIsEmpty) return;

            suggestionProviders
                .Providers
                .Select(provider => provider.GetSuggestions())
                .Aggregate(Observable.Merge)
                .Subscribe(addSuggestions);
        }

        private void addSuggestions(Suggestion suggestions)
        {
            Suggestions.Add(suggestions);
            RaisePropertyChanged(nameof(IsEmpty));
        }

        private async Task startTimeEntry(Suggestion suggestion)
        {
            await dataSource.User
                .Current()
                .Select(user => new StartTimeEntryDTO
                {
                    UserId = user.Id,
                    TaskId = suggestion.TaskId,
                    ProjectId = suggestion.ProjectId,
                    Description = suggestion.Description,
                    WorkspaceId = suggestion.WorkspaceId,
                    StartTime = timeService.CurrentDateTime
                })
                .SelectMany(dataSource.TimeEntries.Start)
                .Do(_ => dataSource.SyncManager.PushSync());
        }
    }
}
