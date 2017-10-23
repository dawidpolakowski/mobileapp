using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Multivac;
using Toggl.Multivac.Models;
using Toggl.Ultrawave.Helpers;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Serialization;
using Toggl.Ultrawave.Exceptions;

namespace Toggl.Ultrawave.ApiClients
{
    internal abstract class BaseApi
    {
        private readonly IApiClient apiClient;
        private readonly IJsonSerializer serializer;

        public static Action<string> ConsoleWriteLine;

        protected HttpHeader AuthHeader { get; }

        protected BaseApi(IApiClient apiClient, IJsonSerializer serializer, Credentials credentials)
        {
            Ensure.Argument.IsNotNull(apiClient, nameof(apiClient));
            Ensure.Argument.IsNotNull(serializer, nameof(serializer));
            Ensure.Argument.IsNotNull(credentials, nameof(credentials));

            this.apiClient = apiClient;
            this.serializer = serializer;
            this.AuthHeader = credentials.Header;
        }

        protected IObservable<List<TInterface>> CreateListObservable<TModel, TInterface>(Endpoint endpoint, HttpHeader header, List<TModel> entities, SerializationReason serializationReason, IWorkspaceFeatureCollection features = null)
            where TModel : class, TInterface
        {
            var body = serializer.Serialize(entities, serializationReason, features);
            return CreateListObservable<TModel, TInterface>(endpoint, header, body);
        }

        protected IObservable<List<TInterface>> CreateListObservable<TModel, TInterface>(Endpoint endpoint, HttpHeader header, string body = "")
            where TModel : class, TInterface
            => CreateListObservable<TModel, TInterface>(endpoint, new[] { header }, body);


        protected IObservable<List<TInterface>> CreateListObservable<TModel, TInterface>(Endpoint endpoint, IEnumerable<HttpHeader> headers, string body = "")
            where TModel : class, TInterface
        {
            var observable = CreateObservable<List<TModel>>(endpoint, headers, body);
            return observable.Select(items => items?.ToList<TInterface>());
        }

        protected IObservable<T> CreateObservable<T>(Endpoint endpoint, HttpHeader header, T entity, SerializationReason serializationReason, IWorkspaceFeatureCollection features = null) {
            var body = serializer.Serialize<T>(entity, serializationReason, features);
            return CreateObservable<T>(endpoint, header, body);
        }
        
        protected IObservable<T> CreateObservable<T>(Endpoint endpoint, HttpHeader header, string body = "")
            => CreateObservable<T>(endpoint, new[] { header }, body);

        protected IObservable<T> CreateObservable<T>(Endpoint endpoint, IEnumerable<HttpHeader> headers, string body = "")
        {
            //var request = new Request(body, endpoint.Url, headers, endpoint.Method);
            //return Observable.Create<T>(async observer =>
            //{
            //    ConsoleWriteLine($"1: send request: [${endpoint.Method}] ${endpoint.Url}");
            //    var response = await apiClient.Send(request).ConfigureAwait(false);
            //    ConsoleWriteLine($"2: receive response: [${endpoint.Method}] ${endpoint.Url} -- *${response.StatusCode}*");
            //    if (response.IsSuccess)
            //    {
            //        try
            //        {
            //            ConsoleWriteLine($"3: response was successful: [${endpoint.Method}] ${endpoint.Url}");
            //            var data = !string.IsNullOrEmpty(response.RawData)
            //                ? await Task.Run(() => serializer.Deserialize<T>(response.RawData)).ConfigureAwait(false)
            //                : default(T);
            //            ConsoleWriteLine($"4: response deserialized: [${endpoint.Method}] ${endpoint.Url}");
            //            observer.OnNext(data);
            //            observer.OnCompleted();
            //        }
            //        catch
            //        {
            //            ConsoleWriteLine($"?: deserialization failed: [${endpoint.Method}] ${endpoint.Url}");
            //            observer.OnError(new DeserializationException<T>(response.RawData));
            //        }
            //    }
            //    else
            //    {
            //        ConsoleWriteLine($"3: request was not succeessful: [${endpoint.Method}] ${endpoint.Url}");
            //        var exception = ApiExceptions.ForResponse(response);
            //        observer.OnError(exception);
            //    }
            //    ConsoleWriteLine($"9: exit method: [${endpoint.Method}] ${endpoint.Url}");
            //});
            
            return Observable.Create<T>(async observer =>
            {
                await Task.Delay(2000);
                observer.OnError(new Exception("Blah blah blah"));
            });
        }
    }
}
