using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using FluentAssertions;
using FsCheck.Xunit;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Sync.States;
using Toggl.Multivac.Extensions;
using Xunit;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class TheDeadlocks
    {
        public abstract class BaseDeadlockTests
        {
            protected ISyncManager SyncManager;
            protected TransitionHandlerProvider Transitions;
            protected IScheduler Scheduler;
            protected IStateMachine StateMachine;
            protected ISyncStateQueue Queue;
            protected IStateMachineOrchestrator Orchestrator;
            protected StateMachineEntryPoints EntryPoints;

            public BaseDeadlockTests(IScheduler scheduler)
            {
                Scheduler = scheduler;
                Reset();
            }

            protected void Reset()
            {
                Queue = new SyncStateQueue();
                Transitions = new TransitionHandlerProvider();
                Scheduler = new TestScheduler();
                StateMachine = new StateMachine(Transitions, Scheduler);
                EntryPoints = new StateMachineEntryPoints();
                Orchestrator = new StateMachineOrchestrator(StateMachine, EntryPoints);
                SyncManager = new SyncManager(Queue, Orchestrator);
            }

            protected StateResult PreparePullTransitions(int n)
                => PrepareTransitions(EntryPoints.StartPullSync, n);

            protected StateResult PreparePushTransitions(int n)
                => PrepareTransitions(EntryPoints.StartPushSync, n);

            protected StateResult PrepareTransitions(StateResult entryPoint, int n)
            {
                var lastResult = entryPoint;
                for (int i = 0; i < Math.Abs(n) + 1; i++)
                {
                    var nextResult = new StateResult();
                    Func<IObservable<ITransition>> transition = () => Observable.Return(new Transition(nextResult));

                    Transitions.ConfigureTransition(lastResult, transition);

                    lastResult = nextResult;
                }

                return lastResult;
            }

            protected void PrepareFailingTransition(StateResult lastResult)
            {
                Func<IObservable<ITransition>> failingTransition = () => Observable.Throw<ITransition>(new TestException());
                Transitions.ConfigureTransition(lastResult, failingTransition);
            }
        }

        public sealed class TheSyncManager : BaseDeadlockTests
        {
            public TheSyncManager() : base(Substitute.For<IScheduler>())
            {
            }

            [Fact]
            public void DoNotGetStuckInADeadlockWhenThereAreNoTransitionHandlersForFullSync()
            {
                var firstStateOfSecondFullSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondFullSync.Should().NotBe(Sleep);
            }

            [Fact]
            public void DoNotGetStuckInADeadlockWhenThereAreNoTransitionHandlersForPushSync()
            {
                var firstStateOfSecondFullSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondFullSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenThereAreSomeTransitionHandlersForFullSync(int n)
            {
                Reset();
                PreparePullTransitions(n);
                PreparePushTransitions(n);

                var firstStateOfSecondFullSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondFullSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenThereAreSomeTransitionHandlersForPushSync(int n)
            {
                Reset();
                PreparePushTransitions(n);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterPush();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenSomeTransitionFailsForFullSync(int n)
            {
                Reset();
                var lastResult = PreparePullTransitions(n);
                PrepareFailingTransition(lastResult);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenSomeTransitionFailsForPushSync(int n)
            {
                Reset();
                var lastResult = PreparePushTransitions(n);
                PrepareFailingTransition(lastResult);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterPush();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            private SyncState getFirstStateOfSecondSyncAfterFull()
                => getFirstStateOfSecondSync(SyncManager.ForceFullSync);

            private SyncState getFirstStateOfSecondSyncAfterPush()
                => getFirstStateOfSecondSync(SyncManager.PushSync);

            private SyncState getFirstStateOfSecondSync(Func<IObservable<SyncState>> sync)
            {
                var stateObservable = sync();
                stateObservable.SkipWhile(state =>
                {
                    return state != Sleep;
                }).Wait();
                var secondSyncStateObservable = sync();
                return secondSyncStateObservable.FirstAsync().Wait();
            }
        }



            }

            {
            }



            {
            }
        }
    }
}
