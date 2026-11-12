namespace ShadowShift
{
    public abstract class CharacterAirborneState : CharacterState
    {
        protected CharacterAirborneState(PlayerCharacter character, StateMachine<CharacterState> stateMachine, string name)
            : base(character, stateMachine, name) { }

        public override void Update()
        {
            base.Update();

            if (!Character.IsGrounded())
                return;

            StateMachine.ChangeState(Character.MoveInput == 0f ? Character.IdleState : Character.MoveState);
            if (Character.JumpRequested)
                StateMachine.ChangeState(Character.JumpState);
        }
    }
}
