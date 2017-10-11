using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;
using Client = Toggl.Ultrawave.Models.Client;
using UserModel = Toggl.Ultrawave.Models.User;

namespace Toggl.Ultrawave.Tests.Integration
{
    //public sealed class ClientsApiTests
    //{
    //    private static Expression<Func<IClient, bool>> clientWithSameIdNameAndWorkspaceAs(IClient client)
    //        => c => c.Id == client.Id && c.Name == client.Name && c.WorkspaceId == client.WorkspaceId;

    //    public sealed class TheGetAllMethod : AuthenticatedEndpointBaseTests<List<IClient>>
    //    {
    //        protected override IObservable<List<IClient>> CallEndpointWith(ITogglApi togglApi)
    //            => togglApi.Clients.GetAll();

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsAllClients()
    //        {
    //            var (togglClient, user) = await SetupTestUser();

    //            var firstClient = new Client { Name = "First", WorkspaceId = user.DefaultWorkspaceId };
    //            var firstClientPosted = await togglClient.Clients.Create(firstClient);
    //            var secondClient = new Client { Name = "Second", WorkspaceId = user.DefaultWorkspaceId };
    //            var secondClientPosted = await togglClient.Clients.Create(secondClient);

    //            var clients = await CallEndpointWith(togglClient);

    //            clients.Should().HaveCount(2);
    //            clients.Should().Contain(clientWithSameIdNameAndWorkspaceAs(firstClientPosted));
    //            clients.Should().Contain(clientWithSameIdNameAndWorkspaceAs(secondClientPosted));
    //        }
    //    }

    //    public sealed class TheGetAllSinceMethod : AuthenticatedGetSinceEndpointBaseTests<IClient>
    //    {
    //        protected override IObservable<List<IClient>> CallEndpointWith(ITogglApi togglApi, DateTimeOffset threshold)
    //            => togglApi.Clients.GetAllSince(threshold);

    //        protected override DateTimeOffset AtDateOf(IClient model) => model.At;

    //        protected override IClient MakeUniqueModel(ITogglApi api, IUser user)
    //            => new Client { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId };

    //        protected override IObservable<IClient> PostModelToApi(ITogglApi api, IClient model)
    //            => api.Clients.Create(model);

    //        protected override Expression<Func<IClient, bool>> ModelWithSameAttributesAs(IClient model)
    //            => clientWithSameIdNameAndWorkspaceAs(model);
    //    }

    //    public sealed class TheCreateMethod : AuthenticatedPostEndpointBaseTests<IClient>
    //    {
    //        protected override IObservable<IClient> CallEndpointWith(ITogglApi togglApi)
    //            => Observable.Defer(async () =>
    //            {
    //                var user = await togglApi.User.Get();
    //                var client = new Client { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId };
    //                return CallEndpointWith(togglApi, client);
    //            });

    //        private IObservable<IClient> CallEndpointWith(ITogglApi togglApi, IClient client)
    //            => togglApi.Clients.Create(client);

    //        [Fact, LogTestInfo]
    //        public async Task CreatesNewClient()
    //        {
    //            var (togglClient, user) = await SetupTestUser();
    //            var newClient = new Client { Name = Guid.NewGuid().ToString(), WorkspaceId = user.DefaultWorkspaceId };

    //            var persistedClient = await CallEndpointWith(togglClient, newClient);

    //            persistedClient.Name.Should().Be(newClient.Name);
    //            persistedClient.WorkspaceId.Should().Be(newClient.WorkspaceId);
    //        }
    //    }
    //}
}
