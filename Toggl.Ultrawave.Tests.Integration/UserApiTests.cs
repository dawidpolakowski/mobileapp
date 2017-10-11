using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Tests.Integration.BaseTests;
using Xunit;

namespace Toggl.Ultrawave.Tests.Integration
{
    //public sealed class UserApiTests
    //{
    //    public sealed class TheGetMethod : AuthenticatedEndpointBaseTests<IUser>
    //    {
    //        protected override IObservable<IUser> CallEndpointWith(ITogglApi togglApi)
    //            => togglApi.User.Get();

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidEmail()
    //        {
    //            var (email, password) = await User.CreateEmailPassword();
    //            var credentials = Credentials.WithPassword(email, password);
    //            var api = TogglApiWith(credentials);

    //            var user = await api.User.Get();
    //            user.Email.Should().Be(email.ToString());
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidId()
    //        {
    //            var (togglApi, user) = await SetupTestUser();

    //            var userFromApi = await CallEndpointWith(togglApi);

    //            userFromApi.Id.Should().NotBe(0);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidApiToken()
    //        {
    //            var (togglApi, user) = await SetupTestUser();
    //            var regex = "^[a-fA-F0-9]+$";

    //            var userFromApi = await CallEndpointWith(togglApi);

    //            userFromApi.ApiToken.Should().NotBeNull()
    //                       .And.HaveLength(32)
    //                       .And.MatchRegex(regex);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidDateFormat()
    //        {
    //            var (togglApi, user) = await SetupTestUser();
    //            string[] validFormats =
    //            {
    //                "%m/%d/%Y",
    //                "%d-%m-%Y",
    //                "%m-%d-%Y",
    //                "%Y-%m-%d",
    //                "%d/%m/%Y",
    //                "%d.%m.%Y"
    //            };

    //            var userFromApi = await CallEndpointWith(togglApi);

    //            userFromApi.DateFormat.Should().BeOneOf(validFormats);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidBeginningOfWeek()
    //        {
    //            var (togglApi, user) = await SetupTestUser();

    //            var userFromApi = await CallEndpointWith(togglApi);
    //            var beginningOfWeekInt = (int)userFromApi.BeginningOfWeek;

    //            beginningOfWeekInt.Should().BeInRange(0, 6);
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidDefaultWorkspaceId()
    //        {
    //            var (togglApi, user) = await SetupTestUser();

    //            var userFromApi = await CallEndpointWith(togglApi);
    //            var workspace = await togglApi.Workspaces.GetById(userFromApi.DefaultWorkspaceId);

    //            userFromApi.DefaultWorkspaceId.Should().NotBe(0);
    //            workspace.Should().NotBeNull();
    //        }

    //        [Fact, LogTestInfo]
    //        public async Task ReturnsValidImageUrl()
    //        {
    //            var (togglApi, user) = await SetupTestUser();

    //            var userFromApi = await CallEndpointWith(togglApi);

    //            userFromApi.ImageUrl.Should().NotBeNullOrEmpty();
    //            var uri = new Uri(userFromApi.ImageUrl);
    //            var uriIsAbsolute = uri.IsAbsoluteUri;
    //            uriIsAbsolute.Should().BeTrue();
    //        }
    //    }
    //}
}
