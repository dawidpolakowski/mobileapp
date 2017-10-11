using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Network;
using Xunit;

namespace Toggl.Ultrawave.Tests.Integration.BaseTests
{
    public abstract class AuthenticatedEndpointBaseTests<T> : EndpointTestBase
    {
        protected abstract IObservable<T> CallEndpointWith(ITogglApi togglApi);

        protected Func<Task> CallingEndpointWith(ITogglApi togglApi)
            => async () => await CallEndpointWith(togglApi);

        //[Fact, LogTestInfo]
        //public async Task WorksWithPassword()
        //{
        //    var credentials = await User.Create();

        //    CallingEndpointWith(TogglApiWith(credentials)).ShouldNotThrow();
        //}

        //[Fact, LogTestInfo]
        //public async Task WorksWithApiToken()
        //{
        //    var (_, user) = await SetupTestUser();
        //    var apiTokenCredentials = Credentials.WithApiToken(user.ApiToken);

        //    CallingEndpointWith(TogglApiWith(apiTokenCredentials)).ShouldNotThrow();
        //}

        //[Fact, LogTestInfo]
        //public void FailsForNonExistingUser()
        //{
        //    var email = $"non-existing-email-{Guid.NewGuid()}@ironicmocks.toggl.com".ToEmail();
        //    var wrongCredentials = Credentials.WithPassword(email, "123456789");

        //    CallingEndpointWith(TogglApiWith(wrongCredentials)).ShouldThrow<ApiException>();
        //}

        //[Fact, LogTestInfo]
        //public async Task FailsWithWrongPassword()
        //{
        //    var (email, password) = await User.CreateEmailPassword();
        //    var wrongCredentials = Credentials.WithPassword(email, $"{password}1");

        //    CallingEndpointWith(TogglApiWith(wrongCredentials)).ShouldThrow<ApiException>();
        //}

        //[Fact, LogTestInfo]
        //public void FailsWithWrongApiToken()
        //{
        //    var wrongApiToken = Guid.NewGuid().ToString("N");
        //    var wrongApiTokenCredentials = Credentials.WithApiToken(wrongApiToken);

        //    CallingEndpointWith(TogglApiWith(wrongApiTokenCredentials)).ShouldThrow<ApiException>();
        //}

        //[Fact, LogTestInfo]
        //public void FailsWithoutCredentials()
        //{
        //    CallingEndpointWith(TogglApiWith(Credentials.None)).ShouldThrow<ApiException>();
        //}
    }
}
