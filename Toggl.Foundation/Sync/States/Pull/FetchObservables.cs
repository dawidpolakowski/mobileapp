using System;
using System.Collections.Generic;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant.Models;
using Toggl.Ultrawave.ApiClients;

namespace Toggl.Foundation.Sync.States
{
    internal sealed class FetchObservables
    {
        public ISinceParameters SinceParameters { get; }
        public IObservable<List<IWorkspace>> Workspaces { get; }
        public IObservable<List<IWorkspaceFeatureCollection>> WorkspaceFeatures { get; }
        public IObservable<List<IClient>> Clients { get; }
        public IObservable<List<IProject>> Projects { get; }
        public IObservable<List<ITag>> Tags { get; }
        public IObservable<List<ITask>> Tasks { get; }
        public IObservable<List<ITimeEntry>> TimeEntries { get; }

        public FetchObservables(FetchObservables old, ISinceParameters sinceParameters)
            : this(sinceParameters, old.Workspaces, old.WorkspaceFeatures, old.Clients, old.Projects, old.TimeEntries, old.Tags, old.Tasks)
        {
        }

        public FetchObservables(ISinceParameters sinceParameters,
            IObservable<List<IWorkspace>> workspaces,
            IObservable<List<IWorkspaceFeatureCollection>> workspaceFeatures, 
            IObservable<List<IClient>> clients,
            IObservable<List<IProject>> projects,
            IObservable<List<ITimeEntry>> timeEntries,
            IObservable<List<ITag>> tags,
            IObservable<List<ITask>> tasks
        )
        {
            SinceParameters = sinceParameters;
            Workspaces = workspaces;
            WorkspaceFeatures = workspaceFeatures;
            Clients = clients;
            Projects = projects;
            TimeEntries = timeEntries;
            Tags = tags;
            Tasks = tasks;
        }
    }
}
