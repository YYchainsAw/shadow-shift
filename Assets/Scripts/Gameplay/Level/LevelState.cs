namespace ShadowShift
{
    public abstract class LevelState : State
    {
        protected readonly LevelSession Session;

        protected LevelState(LevelSession session, string name) : base(name)
        {
            Session = session;
        }
    }
}
