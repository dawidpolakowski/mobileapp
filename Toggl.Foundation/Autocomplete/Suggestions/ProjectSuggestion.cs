using System.Collections.Generic;
using System.Linq;
using Toggl.PrimeRadiant.Models;

namespace Toggl.Foundation.Autocomplete.Suggestions
{
    public sealed class ProjectSuggestion : AutocompleteSuggestion
    {
        public static ProjectSuggestion NoProject { get; } = new ProjectSuggestion();

        public static IEnumerable<ProjectSuggestion> FromProjects(
            IEnumerable<IDatabaseProject> projects
        ) => projects.Select(project => new ProjectSuggestion(project));

        public static IEnumerable<ProjectSuggestion> FromProjectsPrependingEmpty(
            IEnumerable<IDatabaseProject> projects) 
        {
            yield return NoProject;

            foreach (var project in projects)
                yield return new ProjectSuggestion(project);
        }

        public long ProjectId { get; }

        public int NumberOfTasks { get; } = 0;

        public IList<TaskSuggestion> Tasks { get; }

        public string ProjectName { get; } = "";

        public string ClientName { get; } = "";

        public string ProjectColor { get; }

        public bool TasksVisible { get; set; }

        private ProjectSuggestion()
        {
            ProjectId = 0;
            ClientName = "";
            ProjectColor = "#A3A3A3";
            ProjectName = Resources.NoProject;
        }

        public ProjectSuggestion(IDatabaseProject project)
        {
            ProjectId = project.Id;
            ProjectName = project.Name;
            ProjectColor = project.Color;
            NumberOfTasks = project.Tasks?.Count() ?? 0;
            ClientName = project.Client?.Name ?? "";
            WorkspaceId = project.WorkspaceId;
            WorkspaceName = project.Workspace?.Name ?? "";
            Tasks = project.Tasks?.Select(task => new TaskSuggestion(task)).ToList() ?? new List<TaskSuggestion>();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ProjectName.GetHashCode() * 397) ^
                       (ProjectColor.GetHashCode() * 397) ^
                       ClientName.GetHashCode();
            }
        }
    }
}
