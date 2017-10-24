using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.Navigation;
using MvvmCross.Core.ViewModels;
using PropertyChanged;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.DTOs;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Multivac;
using static Toggl.Multivac.Extensions.ObservableExtensions;
using static Toggl.Foundation.MvvmCross.Helper.Constants;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class EditTimeEntryViewModel : MvxViewModel<long>
    {
        private readonly ITogglDataSource dataSource;
        private readonly IMvxNavigationService navigationService;
        private readonly ITimeService timeService;

        private readonly HashSet<long> tagIds = new HashSet<long>();

        private IDisposable deleteDisposable;
        private IDisposable tickingDisposable;
        private IDisposable confirmDisposable;

        private long? projectId;
        private long? taskId;
        private long workspaceId;
        private long initialWorkspaceId;

        public long Id { get; set; }

        public string Description { get; set; }

        public string Project { get; set; }

        public string ProjectColor { get; set; }

        public string Client { get; set; }

        public string Task { get; set; }

        [DependsOn(nameof(StartTime), nameof(StopTime))]
        public TimeSpan Duration
            => (StopTime ?? timeService.CurrentDateTime) - StartTime;

        public DateTimeOffset StartTime { get; set; }

        private DateTimeOffset? stopTime;
        public DateTimeOffset? StopTime
        {
            get => stopTime;
            set
            {
                if (stopTime == value) return;
                stopTime = value;
                if (stopTime != null)
                {
                    tickingDisposable?.Dispose();
                    tickingDisposable = null;
                    return;
                }
                subscribeToTimeServiceTicks();
            }
        }

        public List<string> Tags { get; private set; }

        [DependsOn(nameof(Tags))]
        public bool HasTags => Tags?.Any() ?? false;

        public bool Billable { get; set; }

        public string SyncErrorMessage { get; private set; }

        public bool SyncErrorMessageVisible { get; private set; }

        public IMvxCommand DeleteCommand { get; }

        public IMvxCommand ConfirmCommand { get; }

        public IMvxCommand DismissSyncErrorMessageCommand { get; }

        public IMvxAsyncCommand CloseCommand { get; }

        public IMvxAsyncCommand EditDurationCommand { get; }

        public IMvxAsyncCommand SelectStartDateTimeCommand { get; }

        public IMvxAsyncCommand SelectProjectCommand { get; }

        public IMvxAsyncCommand SelectTagsCommand { get; }

        public IMvxCommand ToggleBillableCommand { get; }

        public EditTimeEntryViewModel(ITogglDataSource dataSource, IMvxNavigationService navigationService, ITimeService timeService)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(timeService, nameof(timeService));

            this.dataSource = dataSource;
            this.navigationService = navigationService;
            this.timeService = timeService;

            DeleteCommand = new MvxCommand(delete);
            ConfirmCommand = new MvxCommand(confirm);
            CloseCommand = new MvxAsyncCommand(close);
            EditDurationCommand = new MvxAsyncCommand(editDuration);
            SelectStartDateTimeCommand = new MvxAsyncCommand(selectStartDateTime);
            SelectProjectCommand = new MvxAsyncCommand(selectProject);
            SelectTagsCommand = new MvxAsyncCommand(selectTags);
            DismissSyncErrorMessageCommand = new MvxCommand(dismissSyncErrorMessageCommand);
            ToggleBillableCommand = new MvxCommand(toggleBillable);
        }

        public override void Prepare(long parameter)
        {
            Id = parameter;
        }

        public override async Task Initialize()
        {
            var timeEntry = await dataSource.TimeEntries.GetById(Id);

            Description = timeEntry.Description;
            StartTime = timeEntry.Start;
            StopTime = timeEntry.Stop;
            Billable = timeEntry.Billable;
            Tags = timeEntry.Tags?.Select(tag => tag.Name).ToList() ?? new List<string>();
            Project = timeEntry.Project?.Name;
            ProjectColor = timeEntry.Project?.Color;
            Task = timeEntry.Task?.Name;
            Client = timeEntry.Project?.Client?.Name;
            projectId = timeEntry.Project?.Id ?? 0;
            SyncErrorMessage = timeEntry.LastSyncErrorMessage;
            initialWorkspaceId = workspaceId = timeEntry.WorkspaceId;
            SyncErrorMessageVisible = !string.IsNullOrEmpty(SyncErrorMessage);
            foreach (var tagId in timeEntry.TagIds)
                tagIds.Add(tagId);

            if (StopTime == null)
                subscribeToTimeServiceTicks();
        }

        private void subscribeToTimeServiceTicks()
        {
            tickingDisposable = timeService
                .CurrentDateTimeObservable
                .Subscribe(_ => RaisePropertyChanged(nameof(Duration)));
        }

        private void delete()
        {
            deleteDisposable = dataSource.TimeEntries
                .Delete(Id)
                .Do(_ => dataSource.SyncManager.PushSync())
                .Subscribe(onDeleteError, onDeleteCompleted);
        }

        private void onDeleteCompleted()
        {
            close();
        }

        private void onDeleteError(Exception exception) { }

        private void confirm()
        {
            var dto = new EditTimeEntryDto
            {
                Id = Id,
                Description = Description,
                StartTime = StartTime,
                StopTime = StopTime,
                ProjectId = projectId,
                TaskId = taskId,
                Billable = Billable,
                WorkspaceId = workspaceId,
                TagIds = new List<long>(tagIds)
            };

            confirmDisposable = dataSource.TimeEntries
                                          .Update(dto)
                                          .Do(_ => dataSource.SyncManager.PushSync())
                                          .Subscribe((Exception ex) => close(), () => close());
        }

        private Task close()
            => navigationService.Close(this);

        private async Task selectStartDateTime()
        {
            var currentTime = timeService.CurrentDateTime;
            var maxDate = StopTime == null 
                        ? currentTime 
                        : StopTime.Value > currentTime ? currentTime : StopTime.Value;
            var minDate = maxDate.AddHours(-MaxTimeEntryDurationInHours);

            var parameters = DatePickerParameters.WithDates(StartTime, minDate, maxDate);
            StartTime = await navigationService
                .Navigate<SelectDateTimeViewModel, DatePickerParameters, DateTimeOffset>(parameters)
                .ConfigureAwait(false);
        }

        private async Task selectProject()
        {
            var selectedProjectIdAndTaskId = await navigationService
                .Navigate<SelectProjectViewModel, (long?, long?, long), (long? projectId, long? taskId)>(
                    (projectId, taskId, workspaceId));

            if (selectedProjectIdAndTaskId.projectId == projectId
                && selectedProjectIdAndTaskId.taskId == taskId)
                return;

            projectId = selectedProjectIdAndTaskId.projectId;
            taskId = selectedProjectIdAndTaskId.taskId;

            if (projectId == null)
            {
                Project = Task = Client = ProjectColor = "";
                clearTagsIfNeeded(workspaceId, initialWorkspaceId);
                workspaceId = initialWorkspaceId;
                return;
            }

            var project = await dataSource.Projects.GetById(projectId.Value);
            clearTagsIfNeeded(workspaceId, project.WorkspaceId);
            Project = project.Name;
            Client = project.Client?.Name;
            ProjectColor = project.Color;
            workspaceId = project.WorkspaceId;

            Task = taskId.HasValue ? (await dataSource.Tasks.GetById(taskId.Value)).Name : "";
        }
        
        private async Task editDuration()
        {
            var currentDuration = DurationParameter.WithStartAndStop(StartTime, StopTime);
            var selectedDuration = await navigationService
                .Navigate<EditDurationViewModel, DurationParameter, DurationParameter>(currentDuration)
                .ConfigureAwait(false);
            
            StartTime = selectedDuration.Start;
            StopTime = selectedDuration.Stop;
        }

        private async Task selectTags()
        {
            var tagsToPass = tagIds.ToArray();
            var returnedTags = await navigationService
                .Navigate<SelectTagsViewModel, (long[], long), long[]>(
                    (tagsToPass, workspaceId));

            if (returnedTags.SequenceEqual(tagsToPass))
                return;

            Tags.Clear();
            tagIds.Clear();

            foreach (var tagId in returnedTags)
                tagIds.Add(tagId);

            dataSource.Tags
                .GetAll(tag => tagIds.Contains(tag.Id))
                .Subscribe(onTags);
        }

        private void onTags(IEnumerable<IDatabaseTag> tags)
        {
            tags.Select(tag => tag.Name)
                   .ForEach(Tags.Add);
            RaisePropertyChanged(nameof(Tags));
            RaisePropertyChanged(nameof(HasTags));
        }

        private void dismissSyncErrorMessageCommand()
            => SyncErrorMessageVisible = false;

        private void toggleBillable() 
        {
            Billable = !Billable;
        }

        private void clearTagsIfNeeded(long currenctWorkspaceId, long newWorkspaceId)
        {
            if (currenctWorkspaceId == newWorkspaceId) return;

            Tags.Clear();
            tagIds.Clear();
            RaisePropertyChanged(nameof(Tags));
            RaisePropertyChanged(nameof(HasTags));
        }
    }
}
