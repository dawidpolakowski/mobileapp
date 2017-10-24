using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Toggl.Foundation.Sync;
using Xunit;
using FsCheck.Xunit;
using Toggl.Foundation.Tests.Generators;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class SyncManagerTests
    {
        public abstract class SyncManagerTestBase
        {
            protected Subject<SyncState> OrchestratorSyncComplete { get; } = new Subject<SyncState>();
            protected Subject<SyncState> OrchestratorStates { get; } = new Subject<SyncState>();
            protected ISyncStateQueue Queue { get; } = Substitute.For<ISyncStateQueue>();
            protected IStateMachineOrchestrator Orchestrator { get; } = Substitute.For<IStateMachineOrchestrator>();
            protected SyncManager SyncManager { get; }

            protected SyncManagerTestBase()
            {
                Orchestrator.SyncCompleteObservable.Returns(OrchestratorSyncComplete.AsObservable());
                Orchestrator.StateObservable.Returns(OrchestratorStates.AsObservable());
                SyncManager = new SyncManager(Queue, Orchestrator);
            }
        }

        public sealed class TheConstuctor : SyncManagerTestBase
        {
            [Theory]
            [ClassData(typeof(TwoParameterConstructorTestData))]
            public void ThrowsIfAnyArgumentIsNull(bool useQueue, bool useOrchestrator)
            {
                var queue = useQueue ? Queue : null;
                var orchestrator = useOrchestrator ? Orchestrator : null;

                // ReSharper disable once ObjectCreationAsStatement
                Action constructor = () => new SyncManager(queue, orchestrator);

                constructor.ShouldThrow<ArgumentNullException>();
            }
        }

        public sealed class TheStateProperty : SyncManagerTestBase
        {
            [Property]
            public void ShouldReturnStateFromOrchestrator(int stateValue)
            {
                Orchestrator.State.Returns((SyncState)stateValue);

                SyncManager.State.Should().Be((SyncState)stateValue);
            }
        }

        public sealed class TheStateObservable : SyncManagerTestBase
        {
            [Fact]
            public void ShouldReturnObservableFromOrchestrator()
            {
                var expectedObservable = Substitute.For<IObservable<SyncState>>();
                Orchestrator.StateObservable.Returns(expectedObservable);

                SyncManager.StateObservable.Should().Be(expectedObservable);
            }
        }

        public abstract class ThreadSafeQueingMethodTests : SyncManagerTestBase
        {
            protected abstract void CallMethod();

            [Fact]
            public void AgainTellsQueueToStartSyncAfterCompletingPreviousFullSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();
                Queue.Dequeue().Returns(Sleep);
                OrchestratorSyncComplete.OnNext(Pull);
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.Received().Dequeue();
            }

            [Fact]
            public void AgainTellsQueueToStartSyncAfterCompletingPreviousPushSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.PushSync();
                Queue.Dequeue().Returns(Sleep);
                OrchestratorSyncComplete.OnNext(Push);
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.Received().Dequeue();
            }

            [Fact]
            public async Task DoesNotQueueUntilOtherCompletedEventReturns()
            {
                await ensureMethodIsThreadSafeWith(() => OrchestratorSyncComplete.OnNext(0));
            }

            [Fact]
            public async Task DoesNotQueueUntilOtherCallToPushSyncReturns()
            {
                await ensureMethodIsThreadSafeWith(() => SyncManager.PushSync());
            }

            [Fact]
            public async Task DoesNotQueueUntilOtherCallToForceFullSyncReturns()
            {
                await ensureMethodIsThreadSafeWith(() => SyncManager.ForceFullSync());
            }

            private async Task ensureMethodIsThreadSafeWith(Action otherMethod)
            {
                var endFirstCall = new AutoResetEvent(false);
                var startedFirstCall = new AutoResetEvent(false);
                var startedSecondCall = new AutoResetEvent(false);
                var endedSecondCall = new AutoResetEvent(false);
                var isFirstCall = true;
                Queue.Dequeue().Returns(Sleep).AndDoes(_ =>
                {
                    // the second time we call this we don't want to do anything
                    if (!isFirstCall) return;
                    isFirstCall = false;
                    startedFirstCall.Set();
                    endFirstCall.WaitOne();
                });
                var firstCall = new Thread(() =>
                {
                    otherMethod();
                });
                firstCall.Start();
                // ensure the first call gets inside locked code before starting second one
                startedFirstCall.WaitOne();
                
                var endedSecondCallBool = false;
                var secondCall = new Thread(() =>
                {
                    startedSecondCall.Set();
                    // here is where we call the method that should be blocked until the first returns
                    CallMethod();
                    endedSecondCallBool = true;
                    endedSecondCall.Set();
                });
                secondCall.Start();
                startedSecondCall.WaitOne();

                // ensure that the second call has time to get to the lock
                // this could probably do with much less time, but I rather make sure
                await Task.Delay(100);

                // the first call is still stuck on `endFirstCall`, so the second has to wait
                endedSecondCallBool.Should().BeFalse();

                // end the first call, which should trigger the second to enter the lock,
                // and complete soon after
                endFirstCall.Set();
                endedSecondCall.WaitOne();

                endedSecondCallBool.Should().BeTrue();
            }
        }

        public sealed class TheOrchestratorCompleteObservable : ThreadSafeQueingMethodTests
        {
            protected override void CallMethod()
                => OrchestratorSyncComplete.OnNext(0);

            [Fact]
            public void TellsQueueToStartSync()
            {
                CallMethod();

                Queue.Received().Dequeue();
            }

            [Fact]
            public void DoesNotQueuePullSync()
            {
                CallMethod();

                Queue.DidNotReceive().QueuePullSync();
            }

            [Fact]
            public void DoesNotQueuePushSync()
            {
                CallMethod();

                Queue.DidNotReceive().QueuePullSync();
            }

            [Fact]
            public void TellsQueueToStartOrchestratorWhenAlreadyRunningFullSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.Received().Dequeue();
            }

            [Fact]
            public void TellsQueueToStartOrchestratorWhenAlreadyRunningPushSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.PushSync();
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.Received().Dequeue();
            }

            [Fact]
            public void TellsQueueToStartOrchestratorWhenInSecondPartOfMultiPhaseSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();
                OrchestratorSyncComplete.OnNext(Push);
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.Received().Dequeue();
            }
        }

        public abstract class SyncMethodTests : ThreadSafeQueingMethodTests
        {
            protected override void CallMethod() => CallSyncMethod();

            protected abstract IObservable<SyncState> CallSyncMethod();

            [Property]
            public void ReturnsObservableThatReplaysSyncStatesUntilSleep(bool[] beforeSleep, bool[] afterSleep)
            {
                var observable = CallSyncMethod();
                
                var beforeSleepStates = (beforeSleep ?? Enumerable.Empty<bool>())
                    .Select(b => b ? Push : Pull);
                var afterSleepStates = (afterSleep ?? Enumerable.Empty<bool>())
                    .Select(b => b ? Push : Pull);

                var expectedStates = beforeSleepStates.Concat(new[] { Sleep }).ToList();

                foreach (var states in expectedStates.Concat(afterSleepStates))
                    OrchestratorStates.OnNext(states);

                var actual = observable.ToList().Wait();

                actual.ShouldBeEquivalentTo(expectedStates);
            }

            [Fact]
            public void DoesNotTellQueueToStartOrchestratorWhenAlreadyRunningFullSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.DidNotReceive().Dequeue();
            }

            [Fact]
            public void DoesNotTellQueueToStartOrchestratorWhenAlreadyRunningPushSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.PushSync();
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.DidNotReceive().Dequeue();
            }
            
            [Fact]
            public void DoesNotTellQueueToStartOrchestratorWhenInSecondPartOfMultiPhaseSync()
            {
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();
                OrchestratorSyncComplete.OnNext(Push);
                Queue.ClearReceivedCalls();

                CallMethod();

                Queue.DidNotReceive().Dequeue();
            }
        }

        public sealed class ThePushSyncMethod : SyncMethodTests
        {
            protected override IObservable<SyncState> CallSyncMethod()
                => SyncManager.PushSync();

            [Fact]
            public void TellsQueueToStartSyncAfterQueingPush()
            {
                CallMethod();

                Received.InOrder(() =>
                {
                    Queue.QueuePushSync();
                    Queue.Dequeue();
                });
            }
        }

        public sealed class TheForceFullSyncMethod : SyncMethodTests
        {
            protected override IObservable<SyncState> CallSyncMethod()
                => SyncManager.ForceFullSync();

            [Fact]
            public void TellsQueueToStartSyncAfterQueingPull()
            {
                CallMethod();

                Received.InOrder(() =>
                {
                    Queue.QueuePullSync();
                    Queue.Dequeue();
                });
            }
        }

        public sealed class TheFreezeMethod : SyncManagerTestBase
        {
            [Fact]
            public void DoesNotThrowWhenFreezingAFrozenSyncManager()
            {
                SyncManager.Freeze();

                Action freezing = () => SyncManager.Freeze();

                freezing.ShouldNotThrow();
            }

            [Fact]
            public void FreezingAFrozenSyncManagerImmediatellyReturnsSleep()
            {
                SyncState? firstState = null;
                SyncManager.Freeze();

                var observable = SyncManager.Freeze();
                var subscription = observable.Subscribe(state => firstState = state);

                firstState.Should().Be(Sleep);
            }

            [Fact]
            public void FreezingSyncManagerWhenNoSyncIsRunningImmediatellyReturnsSleep()
            {
                SyncState? firstState = null;

                var observable = SyncManager.Freeze();
                var subscription = observable.Subscribe(state => firstState = state);

                firstState.Should().Be(Sleep);
            }

            [Fact]
            public void RunningPushSyncOnFrozenSyncManagerGoesDirectlyToSleepState()
            {
                SyncManager.Freeze();
                
                SyncManager.PushSync();

                Orchestrator.Received().Start(Arg.Is(Sleep));
            }

            [Fact]
            public void RunningFullSyncOnFrozenSyncManagerGoesDirectlyToSleepState()
            {
                SyncManager.Freeze();

                SyncManager.ForceFullSync();

                Orchestrator.Received().Start(Arg.Is(Sleep));
            }

            [Fact]
            public void KeepsWaitingWhileNoSleepStateOccursAfterFullSync()
            {
                bool finished = false;
                Queue.Dequeue().Returns(Pull);
                SyncManager.ForceFullSync();

                var observable = SyncManager.Freeze().Subscribe(_ => finished = true);
                OrchestratorStates.OnNext(Pull);
                OrchestratorStates.OnNext(Push);

                SyncManager.IsRunningSync.Should().BeTrue();
                finished.Should().BeFalse();
            }

            [Fact]
            public void KeepsWaitingWhileNoSleepStateOccursAfterPushSync()
            {
                bool finished = false;
                Queue.Dequeue().Returns(Push);
                SyncManager.PushSync();

                var observable = SyncManager.Freeze().Subscribe(_ => finished = true);
                OrchestratorStates.OnNext(Push);

                SyncManager.IsRunningSync.Should().BeTrue();
                finished.Should().BeFalse();
            }

            [Fact]
            public void CompletesWhenSleepStateOccursAfterFullSync()
            {
                bool finished = false;
                SyncManager.ForceFullSync();

                var observable = SyncManager.Freeze().Subscribe(_ => finished = true);
                OrchestratorStates.OnNext(Pull);
                OrchestratorStates.OnNext(Push);
                OrchestratorStates.OnNext(Sleep);

                SyncManager.IsRunningSync.Should().BeFalse();
                finished.Should().BeTrue();
            }

            [Fact]
            public void CompletesWhenSleepStateOccursAfterPushSync()
            {
                bool finished = false;
                SyncManager.PushSync();

                var observable = SyncManager.Freeze().Subscribe(_ => finished = true);
                OrchestratorStates.OnNext(Push);
                OrchestratorStates.OnNext(Sleep);

                SyncManager.IsRunningSync.Should().BeFalse();
                finished.Should().BeTrue();
            }

            // test for thread safety
        }
    }
}
