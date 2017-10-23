using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Toggl.Multivac;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.Network;

namespace Toggl.Ultrawave.Tests.Integration
{
    internal static class User
    {
        public static async Task<Credentials> Create()
        {
            BaseApi.ConsoleWriteLine = Console.WriteLine;
            var (email, password) = await CreateEmailPassword();

            return Credentials.WithPassword(email, password);
        }

        public static async Task<(Email email, string password)> CreateEmailPassword()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var email = $"{Guid.NewGuid()}@mocks.toggl.com";
            var password = "123456";

            using (var client = new HttpClient())
            {
                var json = $"{{\"user\":{{\"email\":\"{email}\",\"password\":\"{password}\"}}}}";
                var url = new Uri("https://toggl.space/api/v8/signups");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                await client.PostAsync(url, content);
            }

            return (Email.FromString(email), password);
        }
    }
}
