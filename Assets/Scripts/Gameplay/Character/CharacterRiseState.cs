namespace ShadowShift
{
    public sealed class CharacterRiseState : CharacterAirborneState
    {
        public CharacterRiseState(PlayerCharacter character, StateMachine<CharacterState> stateMachine)
            : base(character, stateMachine, "Rising") { }

        public override void Update()
        {
            if (Character.TryApplyUpdraft())
            {
                Character.ApplyHorizontalMovement();
                return;
            }

            base.Update();
            if (StateMachine.CurrentState == this && Character.Body.linearVelocityY <= 0f)
                StateMachine.ChangeState(Character.FallState);
        }
    }
}
