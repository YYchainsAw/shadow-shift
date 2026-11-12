namespace ShadowShift
{
    public sealed class CharacterJumpState : CharacterAirborneState
    {
        public CharacterJumpState(PlayerCharacter character, StateMachine<CharacterState> stateMachine)
            : base(character, stateMachine, "Jumping") { }

        public override void Enter()
        {
            Character.BeginJump();
        }

        public override void Update()
        {
            base.Update();

            if (StateMachine.CurrentState == this && Character.Body.linearVelocityY <= 0f)
                StateMachine.ChangeState(Character.FallState);
        }
    }
}
