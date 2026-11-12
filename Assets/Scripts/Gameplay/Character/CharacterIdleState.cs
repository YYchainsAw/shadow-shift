namespace ShadowShift
{
    public sealed class CharacterIdleState : CharacterGroundedState
    {
        public CharacterIdleState(PlayerCharacter character, StateMachine<CharacterState> stateMachine)
            : base(character, stateMachine, "Idle") { }
    }
}
