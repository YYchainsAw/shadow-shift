using UnityEngine;

namespace ShadowShift
{
    public sealed class LevelFailedState : LevelState
    {
        private float restartTime;

        public LevelFailedState(LevelSession session) : base(session, "Failed") { }

        public override void Enter()
        {
            restartTime = Time.unscaledTime + 0.5f;
        }

        public override void Update()
        {
            if (Time.unscaledTime >= restartTime)
                Session.RestartLevel();
        }
    }
}
