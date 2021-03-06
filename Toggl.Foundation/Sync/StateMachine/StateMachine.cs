using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Toggl.Multivac;

namespace Toggl.Foundation.Sync
{
    internal sealed class StateMachine : IStateMachine
    {
        private readonly TimeSpan stateTimeout = TimeSpan.FromMinutes(1);

        private readonly Subject<StateMachineEvent> stateTransitions = new Subject<StateMachineEvent>();
        public IObservable<StateMachineEvent> StateTransitions { get; }

        private readonly ITransitionHandlerProvider transitionHandlerProvider;
        private readonly IScheduler scheduler;

        private bool isRunning;

        public StateMachine(ITransitionHandlerProvider transitionHandlerProvider, IScheduler scheduler)
        {
            Ensure.Argument.IsNotNull(transitionHandlerProvider, nameof(transitionHandlerProvider));
            Ensure.Argument.IsNotNull(scheduler, nameof(scheduler));

            this.transitionHandlerProvider = transitionHandlerProvider;
            this.scheduler = scheduler;

            StateTransitions = stateTransitions.AsObservable();
        }

        public void Start(ITransition transition)
        {
            Ensure.Argument.IsNotNull(transition, nameof(transition));
            
            if (isRunning)
                throw new InvalidOperationException("Cannot start state machine if it is already running.");

            isRunning = true;
            onTransition(transition);
        }

        private void performTransition(ITransition transition, TransitionHandler transitionHandler)
        {
            stateTransitions.OnNext(new StateMachineTransition(transition));

            transitionHandler(transition)
                .SingleAsync()
                .Timeout(scheduler.Now + stateTimeout, scheduler)
                .Subscribe(onTransition, onError);
        }

        private void onTransition(ITransition transition)
        {
            var transitionHandler = transitionHandlerProvider.GetTransitionHandler(transition.Result);

            if (transitionHandler == null)
            {
                isRunning = false;
                reachDeadEnd(transition);
                return;
            }

            performTransition(transition, transitionHandler);
        }

        private void onError(Exception exception)
        {
            isRunning = false;
            reportError(exception);
        }

        private void reachDeadEnd(ITransition transition)
        {
            stateTransitions.OnNext(new StateMachineDeadEnd(transition));
        }

        private void reportError(Exception exception)
        {
            stateTransitions.OnNext(new StateMachineError(exception));
        }
    }
}
