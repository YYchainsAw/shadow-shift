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
            Character.Body.gravityScale = gravityBeforeFall * Character.FallGravityMultiplier;
        }

        public override void Exit()
        {
            Character.Body.gravityScale = gravityBeforeFall;
        }
    }
}
