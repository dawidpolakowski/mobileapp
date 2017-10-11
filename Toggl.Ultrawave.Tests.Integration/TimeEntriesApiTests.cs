using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;
using TimeEntry = Toggl.Ultrawave.Models.TimeEntry;

namespace Toggl.Ultrawave.Tests.Integration
{
    //public sealed class TimeEntriesApiTests
    //{
    //    public sealed class TheGetAllMethod : AuthenticatedGetEndpointBaseTests<List<ITimeEntry>>
    //    {
    //        [Fact, LogTestInfo]
    //        public async Task ReturnsAllTimeEntries()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var firstTimeEntry = createTimeEntry(user);
    //            var firstTimeEntryPosted = await togglClient.TimeEntries.Create(firstTimeEntry);
    //            var secondTimeEntry = createTimeEntry(user);
    //            var secondTimeEntryPosted = await togglClient.TimeEntries.Create(secondTimeEntry);

    //            var timeEntries = await CallEndpointWith(togglClient);

    //            timeEntries.Should().HaveCount(2);
    //            timeEntries.Should().Contain(entry =>
    //                entry.Description == firstTimeEntryPosted.Description
    //                && entry.WorkspaceId == user.DefaultWorkspaceId
    //                && entry.Start == firstTimeEntryPosted.Start
    //                && entry.UserId == user.Id);
    //            timeEntries.Should().Contain(entry =>
    //                entry.Description == secondTimeEntryPosted.Description
    //                && entry.WorkspaceId == user.DefaultWorkspaceId
    //                && entry.Start == secondTimeEntryPosted.Start
    //                && entry.UserId == user.Id);
    //        }

    //        protected override IObservable<List<ITimeEntry>> CallEndpointWith(ITogglApi togglApi)
    //            => togglApi.TimeEntries.GetAll();
    //    }
      
    //    public sealed class TheGetAllSinceMethod : AuthenticatedGetSinceEndpointBaseTests<ITimeEntry>
    //    {
    //        protected override IObservable<List<ITimeEntry>> CallEndpointWith(ITogglApi togglApi, DateTimeOffset threshold)
    //            => togglApi.TimeEntries.GetAllSince(threshold);

    //        protected override DateTimeOffset AtDateOf(ITimeEntry model)
    //            => model.At;

    //        protected override ITimeEntry MakeUniqueModel(ITogglApi api, IUser user)
    //            => createTimeEntry(user);

    //        protected override IObservable<ITimeEntry> PostModelToApi(ITogglApi api, ITimeEntry model)
    //            => api.TimeEntries.Create(model);

    //        protected override Expression<Func<ITimeEntry, bool>> ModelWithSameAttributesAs(ITimeEntry model)
    //            => te => te.Id == model.Id && te.Description == model.Description;
    //    }

    //    public sealed class TheCreateMethod : AuthenticatedPostEndpointBaseTests<ITimeEntry>
    //    {
    //        [Fact, LogTestInfo]
    //        public async Task CreatesNewTimeEntry()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var newTimeEntry = createTimeEntry(user);

    //            var persistedTimeEntry = await CallEndpointWith(togglClient, newTimeEntry);

    //            persistedTimeEntry.Description.Should().Be(newTimeEntry.Description);
    //            persistedTimeEntry.WorkspaceId.Should().Be(newTimeEntry.WorkspaceId);
    //            persistedTimeEntry.Billable.Should().Be(false);
    //            persistedTimeEntry.ProjectId.Should().BeNull();
    //            persistedTimeEntry.TaskId.Should().BeNull();
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ThrowsWhenTagIdsIsNull()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var timeEntry = createTimeEntry(user);
    //            timeEntry.TagIds = null;

    //            Action post = () => togglClient.TimeEntries.Create(timeEntry).Wait();

    //            post.ShouldThrow<BadRequestException>();
    //        }

    //        [Theory]
    //        [InlineData(0)]
    //        [InlineData(998)]
    //        [InlineData(-998)]
    //        public async Task CreatesRunningTimeEntryWhenTheDurationIsSetToANegativeTimestampWithinNineHundredNinetyNineHoursInThePast(int hoursOffset)
    //        {
    //            var (togglApi, user) = await SetupTestUser();
    //            var start = DateTimeOffset.Now.AddHours(-hoursOffset);
    //            var duration = -start.ToUnixTimeSeconds();
    //            var timeEntry = new Ultrawave.Models.TimeEntry
    //            {
    //                Description = Guid.NewGuid().ToString(),
    //                WorkspaceId = user.DefaultWorkspaceId,
    //                Start = start,
    //                Stop = null,
    //                Duration = duration,
    //                UserId = user.Id,
    //                TagIds = new List<long>(),
    //                CreatedWith = "IntegrationTests/0.0"
    //            };

    //            var persistedTimeEntry = CallEndpointWith(togglApi, timeEntry).Wait();

    //            persistedTimeEntry.Id.Should().BePositive();
    //        }

    //        [Theory]
    //        [InlineData(1000)]
    //        [InlineData(-1000)]
    //        public async Task FailsCreatingARunningTimeEntryWhenTheDurationIsSetToATimestampOlderThanNineHundredNinetyNineHoursInThePast(int hoursOffset)
    //        {
    //            var (togglApi, user) = await SetupTestUser();
    //            var start = DateTimeOffset.Now.AddHours(-hoursOffset);
    //            var duration = -start.ToUnixTimeSeconds();
    //            var timeEntry = new Ultrawave.Models.TimeEntry
    //            {
    //                Description = Guid.NewGuid().ToString(),
    //                WorkspaceId = user.DefaultWorkspaceId,
    //                Start = start,
    //                Stop = null,
    //                Duration = duration,
    //                UserId = user.Id,
    //                TagIds = new List<long>(),
    //                CreatedWith = "IntegrationTests/0.0"
    //            };

    //            Action creatingTimeEntry = () => CallEndpointWith(togglApi, timeEntry).Wait();

    //            creatingTimeEntry.ShouldThrow<InternalServerErrorException>();
    //        }

    //        protected override IObservable<ITimeEntry> CallEndpointWith(ITogglApi togglApi)
    //            => Observable.Defer(async () =>
    //            {
    //                var user = await togglApi.User.Get();
    //                var timeEntry = createTimeEntry(user);
    //                return CallEndpointWith(togglApi, timeEntry);
    //            });

    //        private IObservable<ITimeEntry> CallEndpointWith(ITogglApi togglApi, TimeEntry client)
    //            => togglApi.TimeEntries.Create(client);
    //    }

    //    public sealed class TheUpdateMethod : AuthenticatedPutEndpointBaseTests<ITimeEntry>
    //    {
    //        [Fact, LogTestInfo]
    //        public async Task UpdatesExistingTimeEntry()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var timeEntry = createTimeEntry(user);
    //            var persistedTimeEntry = await togglClient.TimeEntries.Create(timeEntry);
    //            var timeEntryWithUpdates = new Ultrawave.Models.TimeEntry
    //            {
    //                Id = persistedTimeEntry.Id,
    //                Description = Guid.NewGuid().ToString(),
    //                WorkspaceId = persistedTimeEntry.WorkspaceId,
    //                Billable = persistedTimeEntry.Billable,
    //                Start = persistedTimeEntry.Start,
    //                Stop = persistedTimeEntry.Stop,
    //                TagIds = persistedTimeEntry.TagIds,
    //                UserId = persistedTimeEntry.UserId,
    //                CreatedWith = persistedTimeEntry.CreatedWith
    //            };

    //            var updatedTimeEntry = await togglClient.TimeEntries.Update(timeEntryWithUpdates);

    //            updatedTimeEntry.Id.Should().Be(persistedTimeEntry.Id);
    //            updatedTimeEntry.Description.Should().Be(timeEntryWithUpdates.Description);
    //            updatedTimeEntry.WorkspaceId.Should().Be(persistedTimeEntry.WorkspaceId);
    //            updatedTimeEntry.Billable.Should().Be(false);
    //            updatedTimeEntry.ProjectId.Should().BeNull();
    //            updatedTimeEntry.TaskId.Should().BeNull();
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task AddTagsToAnExistingTimeEntry()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var timeEntry = createTimeEntry(user);
    //            var persistedTimeEntry = await togglClient.TimeEntries.Create(timeEntry);
    //            var tag = await togglClient.Tags.Create(new Models.Tag { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId });
    //            var timeEntryWithUpdates = new TimeEntry
    //            {
    //                Id = persistedTimeEntry.Id,
    //                Description = Guid.NewGuid().ToString(),
    //                WorkspaceId = persistedTimeEntry.WorkspaceId,
    //                Billable = persistedTimeEntry.Billable,
    //                Start = persistedTimeEntry.Start,
    //                Stop = persistedTimeEntry.Stop,
    //                TagIds = new List<long> { tag.Id },
    //                UserId = persistedTimeEntry.UserId,
    //                CreatedWith = persistedTimeEntry.CreatedWith
    //            };

    //            var updatedTimeEntry = await togglClient.TimeEntries.Update(timeEntryWithUpdates);

    //            updatedTimeEntry.Id.Should().Be(persistedTimeEntry.Id);
    //            updatedTimeEntry.Description.Should().Be(timeEntryWithUpdates.Description);
    //            updatedTimeEntry.WorkspaceId.Should().Be(persistedTimeEntry.WorkspaceId);
    //            updatedTimeEntry.Billable.Should().Be(false);
    //            updatedTimeEntry.ProjectId.Should().BeNull();
    //            updatedTimeEntry.TaskId.Should().BeNull();
    //            updatedTimeEntry.TagIds.Count().Should().Be(1);
    //            updatedTimeEntry.TagIds.First().Should().Be(tag.Id);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ThrowsWhenTagIdsIsNull()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var timeEntry = createTimeEntry(user);
    //            var persistedTimeEntry = await togglClient.TimeEntries.Create(timeEntry);
    //            timeEntry.Id = persistedTimeEntry.Id;
    //            timeEntry.TagIds = null;

    //            Action put = () => togglClient.TimeEntries.Update(timeEntry).Wait();

    //            put.ShouldThrow<BadRequestException>();
    //        }

    //        protected override IObservable<ITimeEntry> CallEndpointWith(ITogglApi togglApi)
    //            => Observable.Defer(async () =>
    //            {
    //                var user = await togglApi.User.Get();
    //                var timeEntry = createTimeEntry(user);
    //                var persistedTimeEntry = await togglApi.TimeEntries.Create(timeEntry);
    //                var timeEntryWithUpdates = new TimeEntry
    //                {
    //                    Id = persistedTimeEntry.Id,
    //                    Description = Guid.NewGuid().ToString(),
    //                    WorkspaceId = persistedTimeEntry.WorkspaceId,
    //                    Billable = persistedTimeEntry.Billable,
    //                    Start = persistedTimeEntry.Start,
    //                    Stop = persistedTimeEntry.Stop,
    //                    TagIds = persistedTimeEntry.TagIds,
    //                    UserId = persistedTimeEntry.UserId,
    //                    CreatedWith = persistedTimeEntry.CreatedWith
    //                };

    //                return CallEndpointWith(togglApi, timeEntryWithUpdates);
    //            });

    //        private IObservable<ITimeEntry> CallEndpointWith(ITogglApi togglApi, TimeEntry timeEntry)
    //            => togglApi.TimeEntries.Update(timeEntry);
    //    }

    //    private static TimeEntry createTimeEntry(IUser user) => new TimeEntry
    //    {
    //        WorkspaceId = user.DefaultWorkspaceId,
    //        Billable = false,
    //        Start = new DateTimeOffset(DateTime.Now - TimeSpan.FromMinutes(5)),
    //        Stop = new DateTimeOffset(DateTime.Now),
    //        Description = Guid.NewGuid().ToString(),
    //        TagIds = new List<long>(),
    //        UserId = user.Id,
    //        CreatedWith = "Ultraware Integration Tests"
    //    };
    //}
}
