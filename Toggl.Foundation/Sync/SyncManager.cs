using System;
using System.Reactive.Linq;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using static Toggl.Foundation.Sync.SyncState;

namespace Toggl.Foundation.Sync
{
    public sealed class SyncManager : ISyncManager
    {
        private readonly object stateLock = new object();
        private readonly ISyncStateQueue queue;
        private readonly IStateMachineOrchestrator orchestrator;

        public bool IsRunningSync { get; private set; }
        
        public SyncState State => orchestrator.State;
        public IObservable<SyncState> StateObservable => orchestrator.StateObservable;
        
        public SyncManager(ISyncStateQueue queue, IStateMachineOrchestrator orchestrator)
        {
            Ensure.Argument.IsNotNull(queue, nameof(queue));
            Ensure.Argument.IsNotNull(orchestrator, nameof(orchestrator));

            this.queue = queue;
            this.orchestrator = orchestrator;
            
            orchestrator.SyncCompleteObservable.Subscribe(syncOperationCompleted);
        }

        public IObservable<SyncState> PushSync()
        {
            lock (stateLock)
            {
                queue.QueuePushSync();
                return startSyncIfNeededAndObserve();
            }
        }

        public IObservable<SyncState> ForceFullSync()
        {
            lock (stateLock)
            {
                queue.QueuePullSync();
                return startSyncIfNeededAndObserve();
            }
        }

        private void syncOperationCompleted(SyncState operation)
        {
            lock (stateLock)
            {
                IsRunningSync = false;
                startSyncIfNeeded();
            }
        }

        private IObservable<SyncState> startSyncIfNeededAndObserve()
        {
            startSyncIfNeeded();

            return syncStatesUntilAndIncludingSleep();
        }

        private void startSyncIfNeeded()
        {
            if (IsRunningSync) return;

            IsRunningSync = queue.StartNextQueuedState(orchestrator) != Sleep;
        }

        private IObservable<SyncState> syncStatesUntilAndIncludingSleep()
            => StateObservable.TakeWhile(s => s != Sleep)
                .Concat(Observable.Return(Sleep))
                .ConnectedReplay();
    }
}
