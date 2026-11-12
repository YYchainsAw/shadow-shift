namespace ShadowShift
{
    public sealed class LevelPlayingState : LevelState
    {
        public LevelPlayingState(LevelSession session) : base(session, "Playing") { }

        public override void Enter()
        {
            Session.ResumeSimulation();
        }

        public override void Update()
        {
            if (Session.PausePressed)
            {
                Session.StateMachine.ChangeState(Session.PausedState);
                return;
            }

            Session.UpdateGameplay();
        }

        public override void Exit()
        {
            Session.SuspendSimulation();
        }
    }
}
