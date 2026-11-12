namespace ShadowShift
{
    public abstract class CharacterGroundedState : CharacterState
    {
        protected CharacterGroundedState(PlayerCharacter character, StateMachine<CharacterState> stateMachine, string name)
            : base(character, stateMachine, name) { }

        public override void Update()
        {
            base.Update();

            if (!Character.IsGrounded())
            {
                StateMachine.ChangeState(Character.FallState);
                return;
            }

            if (Character.JumpRequested)
            {
                StateMachine.ChangeState(Character.JumpState);
                return;
            }

            StateMachine.ChangeState(Character.MoveInput == 0f ? Character.IdleState : Character.MoveState);
        }
    }
}
