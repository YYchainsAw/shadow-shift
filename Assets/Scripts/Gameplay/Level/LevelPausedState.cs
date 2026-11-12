namespace ShadowShift
{
    public sealed class LevelPausedState : LevelState
    {
        public LevelPausedState(LevelSession session) : base(session, "Paused") { }

        public override void Update()
        {
            if (Session.PausePressed)
                Session.StateMachine.ChangeState(Session.PlayingState);
        }
    }
}
