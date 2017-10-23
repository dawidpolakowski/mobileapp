using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Toggl.Ultrawave.Exceptions;
using Toggl.Ultrawave.Helpers;
using Toggl.Ultrawave.Network;
using Xunit;
using static System.Net.HttpStatusCode;
using NotImplementedException = Toggl.Ultrawave.Exceptions.NotImplementedException;

namespace Toggl.Ultrawave.Tests.Exceptions
{
    public sealed class ApiErrorResponsesTests
    {
        public sealed class ClientErrors
        {
            [Theory]
            [MemberData(nameof(ClientErrorsList), MemberType = typeof(ApiErrorResponsesTests))]
            public void ReturnsClientErrorException(HttpStatusCode httpStatusCode, Type expectedExceptionType)
            {
                var request = createRequest(HttpMethod.Get);
                var response = createErrorResponse(httpStatusCode);

                var exception = ApiExceptions.For(request, response);

                exception.Should().BeAssignableTo<ClientErrorException>().And.BeOfType(expectedExceptionType);
            }
        }
        
        public sealed class ServerErrors
        {
            [Theory]
            [MemberData(nameof(ServerErrorsList), MemberType = typeof(ApiErrorResponsesTests))]
            public void ReturnsServerErrorException(HttpStatusCode httpStatusCode, Type expectedExceptionType)
            {
                var request = createRequest(HttpMethod.Get);
                var response = createErrorResponse(httpStatusCode);

                var exception = ApiExceptions.For(request, response);

                exception.Should().BeAssignableTo<ServerErrorException>().And.BeOfType(expectedExceptionType);
            }
        }

        public sealed class UnknownErrors
        {
            [Theory]
            [MemberData(nameof(UnknownErrorsList), MemberType = typeof(ApiErrorResponsesTests))]
            public void ReturnsUnknownApiError(HttpStatusCode httpStatusCode)
            {
                var request = createRequest(HttpMethod.Get);
                var response = createErrorResponse(httpStatusCode);

                var exception = ApiExceptions.For(request, response);

                exception.Should().BeOfType<UnknownApiErrorException>()
                    .Which.HttpCode.Should().Equals(httpStatusCode);
            }
        }

        private static Request createRequest(HttpMethod method)
            => new Request("{\"a\":123}", new Uri("https://integration.tests"), new[] { new HttpHeader("X", "Y") }, method);

        private static Response createErrorResponse(HttpStatusCode code, string rawData = "")
            => new Response(rawData, false, "application/json", code);

        public static IEnumerable<object[]> ClientErrorsList
            => new[]
            {
                new object[] { BadRequest, typeof(BadRequestException) },
                new object[] { Unauthorized, typeof(UnauthorizedException) },
                new object[] { PaymentRequired, typeof(PaymentRequiredException) },
                new object[] { Forbidden, typeof(ForbiddenException) },
                new object[] { NotFound, typeof(NotFoundException) },
                new object[] { Gone, typeof(ApiDeprecatedException) },
                new object[] { RequestEntityTooLarge, typeof(RequestEntityTooLargeException) },
                new object[] { 418, typeof(ClientDeprecatedException) }, // HTTP 418 - I Am a Teapot
                new object[] { 429, typeof(TooManyRequestsException) } // HTTP 429 - Too Many Requests
            };

        public static IEnumerable<object[]> ServerErrorsList
            => new[]
            {
                new object[] { InternalServerError, typeof(InternalServerErrorException) },
                new object[] { NotImplemented, typeof(NotImplementedException) },
                new object[] { BadGateway, typeof(BadGatewayException) },
                new object[] { ServiceUnavailable, typeof(ServiceUnavailableException) },
                new object[] { GatewayTimeout, typeof(GatewayTimeoutException) },
                new object[] { HttpVersionNotSupported, typeof(HttpVersionNotSupportedException) }
            };

        private static IEnumerable<object[]> KnownErrorsList
            => ClientErrorsList.Concat(ServerErrorsList);

        private static bool IsKnownError(int code)
            => KnownErrorsList.Any(item => (int)item[0] == code);

        public static IEnumerable<object[]> UnknownErrorsList
            => Enumerable.Range(400, 200)
                .Where(code => !IsKnownError(code))
                .Select(code => new object[] { (HttpStatusCode)code });
    }
}
