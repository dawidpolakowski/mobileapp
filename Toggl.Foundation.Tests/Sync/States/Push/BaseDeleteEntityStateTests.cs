﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Sync.States;
using Toggl.Multivac.Models;
using Toggl.PrimeRadiant;
using Toggl.Ultrawave;
using Toggl.Ultrawave.Exceptions;
using Xunit;

namespace Toggl.Foundation.Tests.Sync.States
{
    public abstract class BaseDeleteEntityStateTests
    {
        private IStartMethodTestHelper helper;

        public BaseDeleteEntityStateTests(IStartMethodTestHelper helper)
        {
            this.helper = helper;
        }

        [Fact]
        public void ReturnsFailTransitionWhenEntityIsNull()
            => helper.ReturnsFailTransitionWhenEntityIsNull();

        [Theory]
        [MemberData(nameof(ClientExceptions))]
        public void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception)
            => helper.ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(exception);

        [Theory]
        [MemberData(nameof(ServerExceptions))]
        public void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException reason)
            => helper.ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(reason);

        [Fact]
        public void ReturnsUnknownErrorTransitionWhenHttpFailsWithNonApi()
            => helper.ReturnsUnknownErrorTransitionWhenHttpFailsWithNonApiException();

        [Fact]
        public void ReturnsFailTransitionWhenDatabaseOperationFails()
            => helper.ReturnsFailTransitionWhenDatabaseOperationFails();

        [Fact]
        public void ReturnsSuccessfulTransitionWhenEverythingWorks()
            => helper.ReturnsSuccessfulTransitionWhenEverythingWorks();

        [Fact]
        public void CallsDatabaseDeleteOperationWithCorrectParameter()
            => helper.CallsDatabaseDeleteOperationWithCorrectParameter();

        [Fact]
        public void DoesNotDeleteTheEntityLocallyIfTheApiOperationFails()
            => helper.DoesNotDeleteTheEntityLocallyIfTheApiOperationFails();

        public static object[] ClientExceptions()
            => new[]
            {
                new object[] { new BadRequestException() },
                new object[] { new UnauthorizedException() },
                new object[] { new PaymentRequiredException() },
                new object[] { new ForbiddenException() },
                new object[] { new NotFoundException() },
                new object[] { new ApiDeprecatedException() },
                new object[] { new RequestEntityTooLargeException() },
                new object[] { new ClientDeprecatedException() },
                new object[] { new TooManyRequestsException() }
            };

        public static object[] ServerExceptions()
            => new[]
            {
                new object[] { new InternalServerErrorException() },
                new object[] { new BadGatewayException() },
                new object[] { new GatewayTimeoutException() },
                new object[] { new HttpVersionNotSupportedException() },
                new object[] { new ServiceUnavailableException() }
            };

        public interface IStartMethodTestHelper
        {
            void ReturnsFailTransitionWhenEntityIsNull();
            void ReturnsClientErrorTransitionWhenHttpFailsWithClientErrorException(ClientErrorException exception);
            void ReturnsServerErrorTransitionWhenHttpFailsWithServerErrorException(ServerErrorException exception);
            void ReturnsUnknownErrorTransitionWhenHttpFailsWithNonApiException();
            void ReturnsFailTransitionWhenDatabaseOperationFails();
            void ReturnsSuccessfulTransitionWhenEverythingWorks();
            void CallsDatabaseDeleteOperationWithCorrectParameter();
            void DoesNotDeleteTheEntityLocallyIfTheApiOperationFails();
        }

        internal abstract class TheStartMethod<TModel, TApiModel> : BasePushEntityStateTests<TModel, TApiModel>, IStartMethodTestHelper
            where TModel : class, IBaseModel, IDatabaseSyncable, TApiModel
        {
            private ITogglApi api;
            private IRepository<TModel> repository;
    
            public TheStartMethod()
                : this(Substitute.For<ITogglApi>(), Substitute.For<IRepository<TModel>>())
            {
            }

            private TheStartMethod(ITogglApi api, IRepository<TModel> repository)
                : base(api, repository)
            {
                this.api = api;
                this.repository = repository;
            }

            public void ReturnsSuccessfulTransitionWhenEverythingWorks()
            {
                var state = createDeleteState(api, repository);
                var entity = CreateDirtyEntityWithNegativeId();
                var clean = CreateCleanEntityFrom(entity);
                var withPositiveId = CreateCleanWithPositiveIdFrom(entity);
                GetDeleteFunction(api)(Arg.Any<TModel>())
                    .Returns(Observable.Return(Unit.Default));
                repository.Delete(Arg.Any<long>())
                    .Returns(Observable.Return(Unit.Default));

                var transition = state.Start(entity).SingleAsync().Wait();

                transition.Result.Should().Be(state.DeletingFinished);
            }

            public void MakesApiCallWithCorrectParameter()
            {
                var state = createDeleteState(api, repository);
                var entity = CreateDirtyEntityWithNegativeId();
                GetDeleteFunction(api)(entity)
                    .Returns(Observable.Return(Unit.Default));

                state.Start(entity).SingleAsync().Wait();

                GetDeleteFunction(api).Received().Invoke(entity);
            }

            public void CallsDatabaseDeleteOperationWithCorrectParameter()
            {
                var state = createDeleteState(api, repository);
                var entity = CreateDirtyEntityWithNegativeId();
                GetDeleteFunction(api)(entity)
                    .Returns(Observable.Return(Unit.Default));

                state.Start(entity).SingleAsync().Wait();

                repository.Received().Delete(entity.Id);
            }

            public void DoesNotDeleteTheEntityLocallyIfTheApiOperationFails()
            {
                var state = createDeleteState(api, repository);
                var entity = CreateDirtyEntityWithNegativeId();
                PrepareApiCallFunctionToThrow(new TestException());

                state.Start(entity).SingleAsync().Wait();

                repository.DidNotReceive().Delete(Arg.Any<long>());
            }

            protected override void PrepareApiCallFunctionToThrow(Exception e)
                => GetDeleteFunction(api)(Arg.Any<TModel>())
                    .Returns(_ => Observable.Throw<Unit>(e));

            protected override void PrepareDatabaseFunctionToThrow(Exception e)
                => repository.Delete(Arg.Any<long>())
                    .Returns(_ => Observable.Throw<Unit>(e));

            private BaseDeleteEntityState<TModel> createDeleteState(ITogglApi api, IRepository<TModel> repository)
                => CreateState(api, repository) as BaseDeleteEntityState<TModel>;

            protected abstract Func<TModel, IObservable<Unit>> GetDeleteFunction(ITogglApi api);
        }
    }
}
