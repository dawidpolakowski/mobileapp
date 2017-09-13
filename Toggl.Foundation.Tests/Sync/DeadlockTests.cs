using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Sync;
using Toggl.Foundation.Tests.Sync.States;
using Xunit;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Tests.Sync
{
    public sealed class TheDeadlocks
    {
        public sealed class TheSyncManager
        {
            private ISyncManager syncManager;
            private TransitionHandlerProvider transitions;
            private IStateMachine stateMachine;
            private ISyncStateQueue queue;
            private IStateMachineOrchestrator orchestrator;
            private StateMachineEntryPoints entryPoints;

            public TheSyncManager()
            {
                reset();
            }

            public void reset()
            {
                queue = new SyncStateQueue();
                transitions = new TransitionHandlerProvider();
                stateMachine = new StateMachine(transitions, Substitute.For<IScheduler>());
                entryPoints = new StateMachineEntryPoints();
                orchestrator = new StateMachineOrchestrator(stateMachine, entryPoints);
                syncManager = new SyncManager(queue, orchestrator);
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
                reset();
                preparePullTransitions(n);
                preparePushTransitions(n);

                var firstStateOfSecondFullSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondFullSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenThereAreSomeTransitionHandlersForPushSync(int n)
            {
                reset();
                preparePushTransitions(n);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterPush();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenSomeTransitionFailsForFullSync(int n)
            {
                reset();
                var lastResult = preparePullTransitions(n);
                prepareFailingTransition(lastResult);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterFull();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            [Property]
            public void DoNotGetStuckInADeadlockWhenSomeTransitionFailsForPushSync(int n)
            {
                reset();
                var lastResult = preparePushTransitions(n);
                prepareFailingTransition(lastResult);

                var firstStateOfSecondSync = getFirstStateOfSecondSyncAfterPush();

                firstStateOfSecondSync.Should().NotBe(Sleep);
            }

            private StateResult preparePullTransitions(int n)
                => prepareTransitions(entryPoints.StartPullSync, n);

            private StateResult preparePushTransitions(int n)
                => prepareTransitions(entryPoints.StartPushSync, n);

            private StateResult prepareTransitions(StateResult entryPoint, int n)
            {
                var lastResult = entryPoint;
                for (int i = 0; i < Math.Abs(n) + 1; i++)
                {
                    var nextResult = new StateResult();
                    Func<IObservable<ITransition>> transition = () => Observable.Return(new Transition(nextResult));

                    transitions.ConfigureTransition(lastResult, transition);

                    lastResult = nextResult;
                }

                return lastResult;
            }

            private void prepareFailingTransition(StateResult lastResult)
            {
                Func<IObservable<ITransition>> failingTransition = () => Observable.Throw<ITransition>(new TestException());
                transitions.ConfigureTransition(lastResult, failingTransition);
            }

            private SyncState getFirstStateOfSecondSyncAfterFull()
                => getFirstStateOfSecondSync(syncManager.ForceFullSync);

            private SyncState getFirstStateOfSecondSyncAfterPush()
                => getFirstStateOfSecondSync(syncManager.PushSync);

            private SyncState getFirstStateOfSecondSync(Func<IObservable<SyncState>> sync)
            {
                var stateObservable = sync();
                stateObservable.SkipWhile(state => state != Sleep).Wait();
                var secondSyncStateObservable = sync();
                return secondSyncStateObservable.FirstAsync().Wait();
            }
        }
    }
}
