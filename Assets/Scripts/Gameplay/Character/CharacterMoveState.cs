namespace ShadowShift
{
    public sealed class CharacterMoveState : CharacterGroundedState
    {
        public CharacterMoveState(PlayerCharacter character, StateMachine<CharacterState> stateMachine)
            : base(character, stateMachine, "Moving") { }
    }
}
