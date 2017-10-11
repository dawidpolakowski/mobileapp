using System;
using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Tests.Integration.Helper;

namespace Toggl.Ultrawave.Tests.Integration
{
    //public sealed class WorkspaceFeaturesApiTests
    //{
    //    public sealed class TheGetAllMethod : AuthenticatedEndpointBaseTests<List<IWorkspaceFeatureCollection>>
    //    {
    //        protected override IObservable<List<IWorkspaceFeatureCollection>> CallEndpointWith(ITogglApi togglApi)
    //            => togglApi.WorkspaceFeatures.GetAll();

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsAllWorkspaceFeatures()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var featuresInEnum = Enum.GetValues(typeof(WorkspaceFeatureId));

    //            var workspaceFeatureCollections = await CallEndpointWith(togglClient);
    //            var distinctWorkspacesCount = workspaceFeatureCollections.Select(f => f.WorkspaceId).Distinct().Count();

    //            workspaceFeatureCollections.Should().HaveCount(1);
    //            workspaceFeatureCollections.First().Features.Should().HaveCount(featuresInEnum.Length);
    //            distinctWorkspacesCount.Should().Be(1);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsAllWorkspaceFeaturesForMultipleWorkspaces()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var anotherWorkspace = await WorkspaceHelper.CreateFor(user);

    //            var workspaceFeatureCollection = await CallEndpointWith(togglClient);

    //            workspaceFeatureCollection.Should().HaveCount(2);
    //            workspaceFeatureCollection.Should().Contain(collection => collection.WorkspaceId == user.DefaultWorkspaceId);
    //            workspaceFeatureCollection.Should().Contain(collection => collection.WorkspaceId == anotherWorkspace.Id);
    //        }
    //    }
    //}
}
