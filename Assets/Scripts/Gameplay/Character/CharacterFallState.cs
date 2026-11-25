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
            if (StateMachine.CurrentState != this)
                return;

            // Coyote time: a jump pressed shortly after walking off a ledge still starts.
            if (Character.JumpRequested && Character.CanCoyoteJump)
            {
                StateMachine.ChangeState(Character.JumpState);
                return;
            }

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
