namespace ShadowShift
{
    public sealed class StateMachine<TState> where TState : State
    {
        public TState CurrentState { get; private set; }

        public void Initialize(TState state)
        {
            CurrentState = state;
            CurrentState.Enter();
        }

        public void ChangeState(TState state)
        {
            if (CurrentState == state)
                return;

            CurrentState.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }
    }
}
