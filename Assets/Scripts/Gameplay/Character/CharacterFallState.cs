namespace ShadowShift
{
    public sealed class CharacterFallState : CharacterAirborneState
    {
        private float gravityBeforeFall;

        public CharacterFallState(PlayerCharacter character, StateMachine<CharacterState> stateMachine)
            : base(character, stateMachine, "Falling") { }

        public override void Enter()
        {
            gravityBeforeFall = Character.Body.gravityScale;
            ApplyFormPhysics();
        }

        public override void Update()
        {
            base.Update();
            if (StateMachine.CurrentState == this)
                ApplyFormPhysics();
        }

        public override void Exit()
        {
            Character.Body.gravityScale = gravityBeforeFall;
        }

        internal void ApplyFormPhysics()
        {
            Character.ApplyFallPhysics(gravityBeforeFall);
        }
    }
}
