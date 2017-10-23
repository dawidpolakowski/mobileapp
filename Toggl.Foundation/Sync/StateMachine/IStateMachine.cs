using System;

namespace Toggl.Foundation.Sync
{
    public interface IStateMachine
    {
        IObservable<StateMachineEvent> StateTransitions { get; }
        void Start(ITransition transition);
        void Freeze();
    }
}
