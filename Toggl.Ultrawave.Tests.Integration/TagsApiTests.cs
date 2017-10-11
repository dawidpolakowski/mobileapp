using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Toggl.Ultrawave.Tests.Integration.Helper;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Ultrawave.Tests.Integration
{
    //public sealed class TagsApiTests
    //{
    //    public sealed class TheGetAllMethod : AuthenticatedGetEndpointBaseTests<List<ITag>>
    //    {
    //        protected override IObservable<List<ITag>> CallEndpointWith(ITogglApi togglApi)
    //            => togglApi.Tags.GetAll();

    //        private readonly string[] tags1 =
    //        {
    //            "tag1",
    //            "tag2",
    //            "tag3"
    //        };

    //        private readonly string[] tags2 =
    //        {
    //            "tag3",
    //            "tag4",
    //            "tag5"
    //        };

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsTagsForAllWorkspaces()
    //        {
    //            var (togglApi, user) = await SetupTestUser();
    //            var otherWorkspace = await WorkspaceHelper.CreateFor(user);

    //            await pushTags(togglApi, tags1, user.DefaultWorkspaceId);
    //            await pushTags(togglApi, tags2, otherWorkspace.Id);

    //            var returnedTags = await CallEndpointWith(togglApi);

    //            returnedTags.Should().HaveCount(tags1.Length + tags2.Length);
    //            assertTags(returnedTags, tags1, user.DefaultWorkspaceId);
    //            assertTags(returnedTags, tags2, otherWorkspace.Id);
    //        }

    //        private void assertTags(List<ITag> returnedTags, string[] expectedTags, long expectedWorkspaceId)
    //        {
    //            foreach (var expectedTag in expectedTags)
    //            {
    //                returnedTags.Should().Contain(t => t.Name == expectedTag && t.WorkspaceId == expectedWorkspaceId);
    //            }
    //        }

    //        private async Task pushTags(ITogglApi togglApi, string[] tags, long workspaceId)
    //        {
    //            foreach (var tagName in tags)
    //            {
    //                var tag = new Tag { Name = tagName, WorkspaceId = workspaceId };
    //                await togglApi.Tags.Create(tag);
    //            }
    //        }
    //    }
        

    //    public sealed class TheGetAllSinceMethod : AuthenticatedGetSinceEndpointBaseTests<ITag>
    //    {
    //        protected override IObservable<List<ITag>> CallEndpointWith(ITogglApi togglApi, DateTimeOffset threshold)
    //            => togglApi.Tags.GetAllSince(threshold);

    //        protected override DateTimeOffset AtDateOf(ITag model)
    //            => model.At;

    //        protected override ITag MakeUniqueModel(ITogglApi api, IUser user)
    //            => new Tag { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId };

    //        protected override IObservable<ITag> PostModelToApi(ITogglApi api, ITag model)
    //            => api.Tags.Create(model);

    //        protected override Expression<Func<ITag, bool>> ModelWithSameAttributesAs(ITag model)
    //            => t => isTheSameAs(model, t);
    //    }
    
    //    public sealed class TheCreateMethod : AuthenticatedPostEndpointBaseTests<ITag>
    //    {
    //        protected override IObservable<ITag> CallEndpointWith(ITogglApi togglApi)
    //            => Observable.Defer(async () =>
    //            {
    //                var user = await togglApi.User.Get();
    //                var tag = await createNewTag(user.DefaultWorkspaceId);
    //                return CallEndpointWith(togglApi, tag);
    //            });

    //        private IObservable<ITag> CallEndpointWith(ITogglApi togglApi, ITag tag)
    //            => togglApi.Tags.Create(tag);

    //        [Fact, LogTestInfo]
    //        public async Task CreatesNewTag()
    //        {
    //            var (togglClient, user) = await SetupTestUser();

    //            var tag = await createNewTag(user.DefaultWorkspaceId);
    //            var persistedTag = await CallEndpointWith(togglClient, tag);

    //            persistedTag.Name.Should().Be(tag.Name);
    //            persistedTag.WorkspaceId.Should().Be(user.DefaultWorkspaceId);
    //        }
    //    }

    //    private static async Task<Tag> createNewTag(long workspaceID)
    //        => new Tag { Name = Guid.NewGuid().ToString(), WorkspaceId = workspaceID };

    //    private static bool isTheSameAs(ITag a, ITag b)
    //        => a.Id == b.Id
    //        && a.Name == b.Name
    //        && a.WorkspaceId == b.WorkspaceId;
    //}
}
