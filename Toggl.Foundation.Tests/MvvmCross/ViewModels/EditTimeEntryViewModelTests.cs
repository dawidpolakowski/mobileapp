using System;
using System.Reactive;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Models;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;
using static Toggl.Foundation.MvvmCross.Helper.Constants;
using Toggl.Foundation.DTOs;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class EditTimeEntryViewModelTests
    {
        public abstract class EditTimeEntryViewModelTest : BaseViewModelTests<EditTimeEntryViewModel>
        {
            protected const long Id = 10;

            protected void ConfigureEditedTimeEntry(DateTimeOffset now, bool isRunning = false)
            {
                var te = TimeEntry.Builder.Create(Id)
                    .SetDescription("Something")
                    .SetStart(now.AddHours(-2))
                    .SetAt(now.AddHours(-2))
                    .SetWorkspaceId(11)
                    .SetUserId(12);

                if (!isRunning)
                    te = te.SetStop(now.AddHours(-1));
               
                var observable = Observable.Return(te.Build());

                DataSource.TimeEntries.GetById(Arg.Is(Id)).Returns(observable);
            }

            protected override EditTimeEntryViewModel CreateViewModel()
                => new EditTimeEntryViewModel(DataSource, NavigationService, TimeService);
        }

        public sealed class TheConstructor : EditTimeEntryViewModelTest
        {
            [Theory]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource, bool useNavigationService, bool useTimeService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var timeService = useTimeService ? TimeService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new EditTimeEntryViewModel(dataSource, navigationService, timeService);

                tryingToConstructWithEmptyParameters.ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheCloseCommand : EditTimeEntryViewModelTest
        {
            [Fact]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received().Close(Arg.Is(ViewModel));
            }
        }

        public sealed class TheDeleteCommand : EditTimeEntryViewModelTest
        {
            [Fact]
            public void CallsDeleteOnDataSource()
            {
                ViewModel.DeleteCommand.Execute();

                DataSource.TimeEntries.Received().Delete(Arg.Is(ViewModel.Id));
            }

            [Fact]
            public async Task DeleteCommandInitiatesPushSync()
            {
                ViewModel.DeleteCommand.Execute();

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact]
            public async Task DoesNotInitiatePushSyncWhenDeletingFails()
            {
                DataSource.TimeEntries.Delete(Arg.Any<long>())
                    .Returns(Observable.Throw<Unit>(new Exception()));

                ViewModel.DeleteCommand.Execute();

                await DataSource.SyncManager.DidNotReceive().PushSync();
            }
        }

        public sealed class TheSelectStartDateTimeCommandCommand : EditTimeEntryViewModelTest
        {
            [Property]
            public void CallsTheSelectDateTimeViewModelWithAMaxDateEqualToTheCurrentTimeWhenTheTimeEntryIsRunning(DateTimeOffset now)
            {
                if (DateTimeOffset.MinValue.AddHours(MaxTimeEntryDurationInHours) <= now ||
                    DateTimeOffset.MaxValue.AddHours(-1) >= now) return;

                TimeService.CurrentDateTime.Returns(now);

                var parameterToReturn = now.AddHours(-2);
                NavigationService
                    .Navigate<DatePickerParameters, DateTimeOffset>(typeof(SelectDateTimeViewModel), Arg.Any<DatePickerParameters>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now, true);
                ViewModel.Prepare(Id);

                ViewModel.SelectStartDateTimeCommand.ExecuteAsync().Wait();

                NavigationService
                    .Received()
                    .Navigate<DatePickerParameters, DateTimeOffset>(
                        typeof(SelectDateTimeViewModel),
                        Arg.Is<DatePickerParameters>(p => p.MaxDate == now));
            }

            [Property]
            public void CallsTheSelectDateTimeViewModelWithAMaxDateEqualToTheValueOfTheStopTimeIfTheTimeEntryIsComplete(DateTimeOffset now)
            {
                if (DateTimeOffset.MinValue.AddHours(MaxTimeEntryDurationInHours) <= now ||
                    DateTimeOffset.MaxValue.AddHours(-1) >= now) return;

                TimeService.CurrentDateTime.Returns(now);

                var parameterToReturn = now.AddHours(-2);
                NavigationService
                    .Navigate<DatePickerParameters, DateTimeOffset>(typeof(SelectDateTimeViewModel), Arg.Any<DatePickerParameters>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now);
                ViewModel.Prepare(Id);

                ViewModel.SelectStartDateTimeCommand.ExecuteAsync().Wait();

                NavigationService
                    .Received()
                    .Navigate<DatePickerParameters, DateTimeOffset>(
                        typeof(SelectDateTimeViewModel),
                        Arg.Is<DatePickerParameters>(p => p.MinDate == now.AddHours(-2)));
            }

            [Property]
            public void CallsTheSelectDateTimeViewModelWithAMinDateThatAllows999HoursOfDuration(DateTimeOffset now)
            {
                if (DateTimeOffset.MinValue.AddHours(MaxTimeEntryDurationInHours) <= now ||
                    DateTimeOffset.MaxValue.AddHours(-1) >= now) return;

                TimeService.CurrentDateTime.Returns(now);

                var parameterToReturn = now.AddHours(-2);
                NavigationService
                    .Navigate<DatePickerParameters, DateTimeOffset>(typeof(SelectDateTimeViewModel), Arg.Any<DatePickerParameters>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now);
                ViewModel.Prepare(Id);

                ViewModel.SelectStartDateTimeCommand.ExecuteAsync().Wait();

                NavigationService
                    .Received()
                    .Navigate<DatePickerParameters, DateTimeOffset>(
                        typeof(SelectDateTimeViewModel),
                        Arg.Is<DatePickerParameters>(p => p.MaxDate == p.MaxDate.AddHours(-MaxTimeEntryDurationInHours)));
            }

            [Property]
            public void SetsTheStartDateToTheValueReturnedByTheSelectDateTimeDialogViewModel(DateTimeOffset now)
            {
                if (DateTimeOffset.MinValue.AddHours(MaxTimeEntryDurationInHours) <= now ||
                    DateTimeOffset.MaxValue.AddHours(-1) >= now) return;

                var parameterToReturn = now.AddHours(-2);
                NavigationService
                    .Navigate<DatePickerParameters, DateTimeOffset>(typeof(SelectDateTimeViewModel), Arg.Any<DatePickerParameters>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now);
                ViewModel.Prepare(Id);

                ViewModel.SelectStartDateTimeCommand.ExecuteAsync().Wait();

                ViewModel.StartTime.Should().Be(parameterToReturn);
            }
        }

        public sealed class TheEditDurationCommand : EditTimeEntryViewModelTest
        {
            [Property]
            public void SetsTheStartTimeToTheValueReturnedByTheSelectDateTimeDialogViewModelWhenEditingARunningTimeEntry(DateTimeOffset now)
            {
                var parameterToReturn = DurationParameter.WithStartAndStop(now.AddHours(-3), null);
                NavigationService
                    .Navigate<DurationParameter, DurationParameter>(typeof(EditDurationViewModel), Arg.Any<DurationParameter>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now);
                ViewModel.Prepare(Id);

                ViewModel.EditDurationCommand.ExecuteAsync().Wait();

                ViewModel.StartTime.Should().Be(parameterToReturn.Start);
            }

            [Property]
            public void SetsTheStopTimeToTheValueReturnedByTheSelectDateTimeDialogViewModelWhenEditingACompletedTimeEntry(DateTimeOffset now)
            {
                var parameterToReturn = DurationParameter.WithStartAndStop(now.AddHours(-4), now.AddHours(-3));
                NavigationService
                    .Navigate<DurationParameter, DurationParameter>(typeof(EditDurationViewModel), Arg.Any<DurationParameter>())
                    .Returns(parameterToReturn);
                ConfigureEditedTimeEntry(now);
                ViewModel.Prepare(Id);

                ViewModel.EditDurationCommand.ExecuteAsync().Wait();

                ViewModel.StopTime.Should().Be(parameterToReturn.Stop);
            }
        }

        public sealed class TheConfirmCommand : EditTimeEntryViewModelTest
        {
            [Fact]
            public async Task InitiatesPushSync()
            {
                ViewModel.ConfirmCommand.Execute();

                await DataSource.SyncManager.Received().PushSync();
            }

            [Fact]
            public async Task DoesNotInitiatePushSyncWhenSavingFails()
            {
                DataSource.TimeEntries.Update(Arg.Any<EditTimeEntryDto>())
                    .Returns(Observable.Throw<IDatabaseTimeEntry>(new Exception()));

                ViewModel.ConfirmCommand.Execute();

                await DataSource.SyncManager.DidNotReceive().PushSync();
            }

            [Fact]
            public async Task UpdatesWorkspaceIdIfProjectFromAnotherWorkspaceWasSelected()
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(10);
                timeEntry.WorkspaceId.Returns(11);
                timeEntry.ProjectId.Returns(12);
                DataSource.TimeEntries.GetById(Arg.Is(timeEntry.Id))
                  .Returns(Observable.Return(timeEntry));
                var newProjectId = 20;
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(newProjectId);
                project.WorkspaceId.Returns(21);
                DataSource.Projects.GetById(project.Id)
                    .Returns(Observable.Return(project));
                ViewModel.Prepare(timeEntry.Id);
                await ViewModel.Initialize();
                NavigationService.Navigate<(long?, long?, long), (long?, long?)>(typeof(SelectProjectViewModel), Arg.Any<(long?, long?, long)>())
                    .Returns((newProjectId, null));
                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.ConfirmCommand.Execute();

                await DataSource.TimeEntries.Received().Update(
                    Arg.Is<EditTimeEntryDto>(dto => dto.WorkspaceId == project.WorkspaceId));
            }

            [Fact]
            public async Task DoesNotUpdateWorkspaceIdIfProjectFromTheSameWorkspaceIsSelected()
            {
                var workspaceId = 11;
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(10);
                timeEntry.WorkspaceId.Returns(workspaceId);
                timeEntry.ProjectId.Returns(12);
                DataSource.TimeEntries.GetById(Arg.Is(timeEntry.Id))
                  .Returns(Observable.Return(timeEntry));
                var newProjectId = 20;
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(newProjectId);
                project.WorkspaceId.Returns(workspaceId);
                DataSource.Projects.GetById(project.Id)
                    .Returns(Observable.Return(project));
                ViewModel.Prepare(timeEntry.Id);
                await ViewModel.Initialize();
                NavigationService.Navigate<(long?, long?), (long?, long?)>(typeof(SelectProjectViewModel), Arg.Any<(long?, long?)>())
                    .Returns((newProjectId, null));
                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.ConfirmCommand.Execute();

                await DataSource.TimeEntries.Received().Update(
                    Arg.Is<EditTimeEntryDto>(dto => dto.WorkspaceId == workspaceId));
            }

            [Fact]
            public async Task DoesNotUpdateWorkspaceIdIfNoProjectWasSelected()
            {
                var workspaceId = 11;
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(10);
                timeEntry.WorkspaceId.Returns(workspaceId);
                timeEntry.ProjectId.Returns(12);
                DataSource.TimeEntries.GetById(Arg.Is(timeEntry.Id))
                  .Returns(Observable.Return(timeEntry));
                ViewModel.Prepare(timeEntry.Id);
                await ViewModel.Initialize();
                NavigationService.Navigate<(long?, long?), (long?, long?)>(typeof(SelectProjectViewModel), Arg.Any<(long?, long?)>())
                    .Returns((null, null));
                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.ConfirmCommand.Execute();

                await DataSource.TimeEntries.Received().Update(
                    Arg.Is<EditTimeEntryDto>(dto => dto.WorkspaceId == workspaceId));
            }
        }

        public sealed class TheSelectTagsCommand : EditTimeEntryViewModelTest
        {
            [Property]
            public void NavigatesToTheSelectTagsViewModelPassingCurrentTagIds(NonNegativeInt[] nonNegativeInts)
            {
                var tagIds = nonNegativeInts.Select(i => (long)i.Get)
                    .Distinct();
                long id = 13;
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.TagIds.Returns(tagIds);
                DataSource.TimeEntries.GetById(Arg.Is(id)).Returns(Observable.Return(timeEntry));
                ViewModel.Prepare(id);
                ViewModel.Initialize().Wait();

                ViewModel.SelectTagsCommand.ExecuteAsync().Wait();

                NavigationService
                    .Received()
                    .Navigate<long[], long[]>(
                        Arg.Is(typeof(SelectTagsViewModel)),
                        Arg.Is<long[]>(ids => ids.SequenceEqual(tagIds)))
                    .Wait();
            }

            [Property]
            public void QueriesTheDataSourceForReturnedTagIds(
                NonEmptyArray<NonNegativeInt> nonNegativeInts, long[] otherIds)
            {
                var tagIds = nonNegativeInts.Get
                    .Select(i => (long)i.Get)
                    .ToArray();
                var tags = tagIds.Select(createTag);
                var otherTags = otherIds.Select(createTag);
                DataSource.Tags.GetAll(Arg.Any<Func<IDatabaseTag, bool>>())
                    .Returns(Observable.Return(tags));
                NavigationService
                    .Navigate<long[], long[]>(Arg.Is(typeof(SelectTagsViewModel)), Arg.Any<long[]>())
                    .Returns(Task.FromResult(tagIds));
                ViewModel.Initialize().Wait();

                ViewModel.SelectTagsCommand.ExecuteAsync().Wait();

                DataSource.Tags.Received()
                    .GetAll(Arg.Is<Func<IDatabaseTag, bool>>(
                        func => ensureFuncWorksAsExpected(func, tags, otherTags)))
                    .Wait();
            }

            private bool ensureFuncWorksAsExpected(
                Func<IDatabaseTag, bool> func,
                IEnumerable<IDatabaseTag> tags,
                IEnumerable<IDatabaseTag> otherTags)
            {
                var tagIdHashSet = new HashSet<long>(tags.Select(tag => tag.Id));
                foreach (var tag in tags)
                    if (!func(tag))
                        return false;

                foreach (var otherTag in otherTags)
                {
                    if (tagIdHashSet.Contains(otherTag.Id))
                        continue;
                    if (func(otherTag))
                        return false;
                }

                return true;
            }

            [Property]
            public void SetsTheReturnedTags(NonEmptyArray<NonNegativeInt> nonNegativeInts)
            {
                var tagIds = nonNegativeInts.Get
                    .Select(i => (long)i.Get)
                    .ToArray();
                var tags = tagIds.Select(createTag);
                var tagNames = tags.Select(tag => tag.Name);
                ViewModel.Initialize().Wait();
                DataSource.Tags.GetAll(Arg.Any<Func<IDatabaseTag, bool>>())
                    .Returns(Observable.Return(tags));
                NavigationService
                    .Navigate<long[], long[]>(Arg.Is(typeof(SelectTagsViewModel)), Arg.Any<long[]>())
                    .Returns(Task.FromResult(tagIds));

                ViewModel.SelectTagsCommand.ExecuteAsync().Wait();

                ViewModel.Tags.Should()
                         .HaveCount(tags.Count()).And
                         .OnlyContain(tag => tagNames.Contains(tag));
            }

            private IDatabaseTag createTag(long id)
            {
                var tag = Substitute.For<IDatabaseTag>();
                tag.Id.Returns(id);
                tag.Name.Returns($"Tag{id}");
                return tag;
            }
        }

        public sealed class TheDismissSyncErrorMessageCommand : EditTimeEntryViewModelTest
        {
            [Theory]
            [InlineData(true)]
            [InlineData(false)]
            public async Task SetsSyncErrorMessageVisiblePropertyToFalse(bool initialValue)
            {
                var errorMessage = initialValue ? "Some error" : null;
                var id = 13;
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.LastSyncErrorMessage.Returns(errorMessage);
                DataSource.TimeEntries.GetById(Arg.Is<long>(id)).Returns(Observable.Return(timeEntry));
                ViewModel.Prepare(id);
                await ViewModel.Initialize();

                ViewModel.DismissSyncErrorMessageCommand.Execute();

                ViewModel.SyncErrorMessageVisible.Should().BeFalse();
            }
        }

        public sealed class TheInitializeMethod : EditTimeEntryViewModelTest
        {
            private readonly IDatabaseTimeEntry timeEntry;

            public TheInitializeMethod()
            {
                timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(Id);
                DataSource.TimeEntries.GetById(Arg.Is<long>(Id)).Returns(Observable.Return(timeEntry));
            }

            [Property]
            public void SetsTheSyncErrorMessageProperty(string errorMessage)
            {
                timeEntry.LastSyncErrorMessage.Returns(errorMessage);
                ViewModel.Prepare(Id);

                ViewModel.Initialize().Wait();

                ViewModel.SyncErrorMessage.Should().Be(errorMessage);
            }

            [Theory]
            [InlineData("Some error", true)]
            [InlineData("", false)]
            [InlineData(null, false)]
            public async Task SetsTheSyncErrorMessageVisibleProperty(
                string errorMessage, bool expectedVisibility)
            {
                timeEntry.LastSyncErrorMessage.Returns(errorMessage);
                ViewModel.Prepare(Id);

                await ViewModel.Initialize();

                ViewModel.SyncErrorMessageVisible.Should().Be(expectedVisibility);
            }
        }

        public sealed class TheSelectProjectCommand : EditTimeEntryViewModelTest
        {
            private async Task prepare(
                long? projectId = null,
                string projectName = null,
                string projectColor = null,
                string clientName = null,
                long? taskId = null,
                string taskName = null)
            {
                await prepareTimeEntry(10);

                if (projectId.HasValue)
                    prepareProject(projectId.Value, projectName, projectColor, clientName);

                if (taskId.HasValue)
                    prepareTask(taskId.Value, taskName);

                NavigationService.Navigate<(long?, long?, long), (long?, long?)>(typeof(SelectProjectViewModel), Arg.Any<(long?, long?, long)>())
                    .Returns((projectId, taskId));
            }

            private async Task prepareTimeEntry(long id)
            {
                var timeEntry = Substitute.For<IDatabaseTimeEntry>();
                timeEntry.Id.Returns(id);
                timeEntry.Project.Name.Returns(Guid.NewGuid().ToString());
                timeEntry.Project.Color.Returns(Guid.NewGuid().ToString());
                timeEntry.Task.Name.Returns(Guid.NewGuid().ToString());
                timeEntry.Project.Client.Name.Returns(Guid.NewGuid().ToString());
                DataSource.TimeEntries.GetById(Arg.Is(id))
                    .Returns(Observable.Return(timeEntry));
                ViewModel.Prepare(id);
                await ViewModel.Initialize();
            }

            private void prepareProject(
                long projectId, string projectName, string projectColor, string clientName)
            {
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(projectId);
                project.Name.Returns(projectName);
                project.Color.Returns(projectColor);
                project.Client.Name.Returns(clientName);
                DataSource.Projects.GetById(Arg.Is(projectId))
                    .Returns(Observable.Return(project));
            }

            private void prepareTask(long taskId, string taskName)
            {
                var task = Substitute.For<IDatabaseTask>();
                task.Id.Returns(taskId);
                task.Name.Returns(taskName);
                DataSource.Tasks.GetById(Arg.Is(task.Id))
                    .Returns(Observable.Return(task));
            }

            [Fact]
            public async Task SetsTheProject()
            {
                var projectName = "Some other project";
                await prepare(projectId: 11, projectName: projectName);

                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.Project.Should().Be(projectName);
            }

            [Fact]
            public async Task SetsTheTask()
            {
                var taskName = "Some task";
                await prepare(
                    projectId: 11,
                    projectName: "Project",
                    taskId: 12,
                    taskName: taskName);

                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.Task.Should().Be(taskName);
            }

            [Fact]
            public async Task SetsTheClient()
            {
                var clientName = "Some client";
                await prepare(
                    projectId: 11,
                    projectName: "Project",
                    clientName: clientName);

                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.Client.Should().Be(clientName);
            }

            [Fact]
            public async Task SetsTheColor()
            {
                var projectColor = "123456";
                await prepare(
                    projectId: 11,
                    projectName: "Project",
                    projectColor: projectColor);

                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.ProjectColor.Should().Be(projectColor);
            }

            [Fact]
            public async Task RemovesTheTaskIfNoTaskWasSelected()
            {
                await prepare(11, "Some project");

                await ViewModel.SelectProjectCommand.ExecuteAsync();

                ViewModel.Task.Should().BeEmpty();
            }
        }
    }
}
