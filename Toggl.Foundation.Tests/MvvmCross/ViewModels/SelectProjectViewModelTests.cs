using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Autocomplete;
using Toggl.Foundation.Autocomplete.Suggestions;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Generators;
using Toggl.PrimeRadiant.Models;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class SelectProjectViewModelTests
    {
        public abstract class SelectProjectViewModelTest : BaseViewModelTests<SelectProjectViewModel>
        {
            protected override SelectProjectViewModel CreateViewModel()
            => new SelectProjectViewModel(DataSource, NavigationService, DialogService);
        }

        public sealed class TheConstructor : SelectProjectViewModelTest
        {
            [Theory]
            [ClassData(typeof(ThreeParameterConstructorTestData))]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useDataSource, bool useNavigationService, bool useDialogService)
            {
                var dataSource = useDataSource ? DataSource : null;
                var navigationService = useNavigationService ? NavigationService : null;
                var dialogService = useDialogService ? DialogService : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new SelectProjectViewModel(dataSource, navigationService, dialogService);

                tryingToConstructWithEmptyParameters
                    .ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheCloseCommand : SelectProjectViewModelTest
        {
            [Fact]
            public async Task ClosesTheViewModel()
            {
                await ViewModel.CloseCommand.ExecuteAsync();

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<(long?, long?)>());
            }

            [Property]
            public void ReturnsTheSameProjectIdThatWasPassedToTheViewModel(long? projectId)
            {
                ViewModel.Prepare((projectId, 10, 11));

                ViewModel.CloseCommand.ExecuteAsync().Wait();

                NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.projectId == projectId)).Wait();
            }

            [Property]
            public void ReturnsTheSameTaskIdThatWasPassedToTheViewModel(long? taskId)
            {
                ViewModel.Prepare((10, taskId, 11));

                ViewModel.CloseCommand.ExecuteAsync().Wait();

                NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.taskId == taskId)).Wait();
            }
        }

        public sealed class TheSelectProjectCommand : SelectProjectViewModelTest
        {
            [Fact]
            public async Task ClosesTheViewModel()
            {
                ViewModel.SelectProjectCommand.Execute(ProjectSuggestion.NoProject);

                await NavigationService.Received()
                    .Close(Arg.Is(ViewModel), Arg.Any<(long?, long?)>());
            }

            [Fact]
            public async Task ReturnsTheSelectedProjectIdWhenSelectingAProject()
            {
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(13);
                var selectedProject = new ProjectSuggestion(project);

                ViewModel.SelectProjectCommand.Execute(selectedProject);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.projectId == selectedProject.ProjectId));
            }

            [Fact]
            public async Task ReturnsNoTaskIdWhenSelectingAProject()
            {
                var project = Substitute.For<IDatabaseProject>();
                project.Id.Returns(13);
                var selectedProject = new ProjectSuggestion(project);

                ViewModel.SelectProjectCommand.Execute(selectedProject);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.taskId == null));
            }

            [Fact]
            public async Task ReturnsTheSelectedProjectIdWhenSelectingATask()
            {
                var task = Substitute.For<IDatabaseTask>();
                task.Id.Returns(13);
                task.ProjectId.Returns(10);
                var selectedTask = new TaskSuggestion(task);

                ViewModel.SelectProjectCommand.Execute(selectedTask);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.projectId == task.ProjectId));
            }

            [Fact]
            public async Task ReturnsTheSelectedTaskIdWhenSelectingATask()
            {
                var task = Substitute.For<IDatabaseTask>();
                task.Id.Returns(13);
                task.ProjectId.Returns(10);
                var selectedTask = new TaskSuggestion(task);

                ViewModel.SelectProjectCommand.Execute(selectedTask);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.taskId == task.Id));
            }

            [Fact] 
            public async Task ReturnsNullIfNoProjectWasSelected()
            {
                ViewModel.SelectProjectCommand.Execute(ProjectSuggestion.NoProject);

                await NavigationService.Received().Close(
                    Arg.Is(ViewModel),
                    Arg.Is<(long? projectId, long? taskId)>(
                        parameter => parameter.projectId == null
                                     && parameter.taskId == null));
            }
        }

        public sealed class TheTextProperty : SelectProjectViewModelTest
        {
            [Fact]
            public async Task WhenChangedQueriesTheAutocompleteProvider()
            {
                var text = "Some text";
                var autocompleteProvider = Substitute.For<IAutocompleteProvider>();
                DataSource.AutocompleteProvider.Returns(autocompleteProvider);
                await ViewModel.Initialize();

                ViewModel.Text = text;

                await autocompleteProvider.Received().Query(Arg.Is(text), Arg.Is(AutocompleteSuggestionType.Projects));
            }
        }

        public sealed class TheSuggestionsProperty : SelectProjectViewModelTest
        {
            private IEnumerable<ProjectSuggestion> getProjectSuggestions(int count, int workspaceId)
            {
                for (int i = 0; i < count; i++)
                {
                    var workspace = Substitute.For<IDatabaseWorkspace>();
                    workspace.Name.Returns($"Workspace{workspaceId}");
                    workspace.Id.Returns(workspaceId);
                    var project = Substitute.For<IDatabaseProject>();
                    project.Name.Returns($"Project{i}");
                    project.Workspace.Returns(workspace);
                    yield return new ProjectSuggestion(project);
                }
            }
            
            [Fact]
            public async Task IsClearedWhenTextIsChanged()
            {
                var oldSuggestions = getProjectSuggestions(3, 1);
                var newSuggestions = getProjectSuggestions(1, 1);
                var autocompleteProvider = Substitute.For<IAutocompleteProvider>();
                var queryText = "Query text";
                autocompleteProvider.Query(Arg.Any<string>(), Arg.Is(AutocompleteSuggestionType.Projects))
                    .Returns(Observable.Return(oldSuggestions));
                autocompleteProvider.Query(Arg.Is(queryText), Arg.Is(AutocompleteSuggestionType.Projects))
                    .Returns(Observable.Return(newSuggestions));
                DataSource.AutocompleteProvider.Returns(autocompleteProvider);
                await ViewModel.Initialize();

                ViewModel.Text = queryText;

                ViewModel.Suggestions.Should().HaveCount(1);
                ViewModel.Suggestions.First().Should().HaveCount(2);
            }

            [Fact]
            public async Task IsPopulatedAfterInitialization()
            {
                var projectSuggestions = getProjectSuggestions(10, 0);
                var suggestionsObservable = Observable.Return(projectSuggestions);
                var autocompleteProvider = Substitute.For<IAutocompleteProvider>();
                autocompleteProvider.Query(Arg.Any<string>(), Arg.Is(AutocompleteSuggestionType.Projects))
                    .Returns(suggestionsObservable);
                DataSource.AutocompleteProvider.Returns(autocompleteProvider);
                
                await ViewModel.Initialize();

                ViewModel.Suggestions.Should().HaveCount(1);
                ViewModel.Suggestions.First().Should().HaveCount(11);
            }

            [Fact]
            public async Task PrependsEmptyProjectToEveryGroup()
            {
                var suggestions = new List<ProjectSuggestion>();
                suggestions.AddRange(getProjectSuggestions(3, 0));
                suggestions.AddRange(getProjectSuggestions(4, 1));
                suggestions.AddRange(getProjectSuggestions(1, 10));
                suggestions.AddRange(getProjectSuggestions(10, 54));
                var suggestionsObservable = Observable.Return(suggestions);
                var autocompleteProvider = Substitute.For<IAutocompleteProvider>();
                autocompleteProvider.Query(Arg.Any<string>(), Arg.Is(AutocompleteSuggestionType.Projects))
                    .Returns(suggestionsObservable);

                await ViewModel.Initialize();

                foreach (var group in ViewModel.Suggestions)
                {
                    group.Cast<ProjectSuggestion>().First().ProjectName.Should().Be(Resources.NoProject);
                }
            }

            [Fact]
            public async Task GroupsProjectsByWorkspace()
            {
                var suggestions = new List<ProjectSuggestion>();
                var workspaceIds = new[] { 0, 1, 10, 54 };
                suggestions.AddRange(getProjectSuggestions(3, workspaceIds[0]));
                suggestions.AddRange(getProjectSuggestions(4, workspaceIds[1]));
                suggestions.AddRange(getProjectSuggestions(1, workspaceIds[2]));
                suggestions.AddRange(getProjectSuggestions(10, workspaceIds[3]));
                var suggestionsObservable = Observable.Return(suggestions);
                var autocompleteProvider = Substitute.For<IAutocompleteProvider>();
                DataSource.AutocompleteProvider.Returns(autocompleteProvider);
                autocompleteProvider.Query(Arg.Any<string>(), Arg.Is(AutocompleteSuggestionType.Projects))
                    .Returns(suggestionsObservable);

                await ViewModel.Initialize();

                ViewModel.Suggestions.Should().HaveCount(4);
                foreach (var suggestionGroup in ViewModel.Suggestions)
                {
                    foreach (var suggestion in suggestionGroup.Cast<ProjectSuggestion>())
                    {
                        if (suggestion.ProjectName == Resources.NoProject)
                            continue;
                        suggestion.WorkspaceName.Should().Be(suggestionGroup.WorkspaceName);
                    }
                }
            }
        }
    }
}
