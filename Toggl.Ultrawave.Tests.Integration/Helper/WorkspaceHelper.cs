using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.ApiClients;
using Toggl.Ultrawave.Models;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Ultrawave.Tests.Integration.Helper
{
    internal static class WorkspaceHelper
    {
        public static Action<string> ConsoleWriteLine;

        public static async Task<Workspace> CreateFor(IUser user)
        {
            BaseApi.ConsoleWriteLine = Console.WriteLine;
            var newWorkspaceName = $"{Guid.NewGuid()}";
            var json = $"{{\"name\": \"{newWorkspaceName}\"}}";

            var responseBody = await makeRequest("https://toggl.space/api/v9/workspaces", HttpMethod.Post, user, json);

            var jsonSerializer = new JsonSerializer();
            var deserialized = jsonSerializer.Deserialize<Workspace>(responseBody);
            Console.WriteLine($"WH3: response deserialization: CreateFor");
            return deserialized;
        }

        public static async Task SetSubscription(IUser user, long workspaceId, PricingPlans plan)
        {
             BaseApi.ConsoleWriteLine = Console.WriteLine;
             var json = $"{{\"pricing_plan_id\":{(int)plan}}}";

             var result = await makeRequest($"https://toggl.space/api/v9/workspaces/{workspaceId}/subscriptions", HttpMethod.Post, user, json);
             Console.WriteLine($"WH3: got response: SetSubscription");
        }

        public static async Task<List<int>> GetAllAvailablePricingPlans(IUser user)
        {
            BaseApi.ConsoleWriteLine = Console.WriteLine;
            var response = await makeRequest($"https://toggl.space/api/v9/workspaces/{user.DefaultWorkspaceId}/plans", HttpMethod.Get, user, null);
            Console.WriteLine($"WH3: got response: GetAllAvailablePricingPlans");
            var matches = Regex.Matches(response, "\\\"pricing_plan_id\\\":\\s*(?<id>\\d+),");
            return matches.Cast<Match>().SelectMany(match => match.Groups["id"].Captures.Cast<Capture>().Select(capture => int.Parse(capture.Value))).ToList();
        }

        private static async Task<string> makeRequest(string endpoint, HttpMethod method, IUser user, string json)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var requestMessage = AuthorizedRequestBuilder.CreateRequest(
                Credentials.WithApiToken(user.ApiToken), endpoint, method);

            if (json != null)
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(3);
                Console.WriteLine($"WH1: send request: [${method}] ${endpoint}");
                var response = await client.SendAsync(requestMessage);
                Console.WriteLine($"WH2: received response: [${method}] ${endpoint} -- *${response.StatusCode}*");
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
}
