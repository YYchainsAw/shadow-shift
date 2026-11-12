namespace ShadowShift
{
    public abstract class CharacterState : State
    {
        protected readonly PlayerCharacter Character;
        protected readonly StateMachine<CharacterState> StateMachine;

        protected CharacterState(PlayerCharacter character, StateMachine<CharacterState> stateMachine, string name)
            : base(name)
        {
            Character = character;
            StateMachine = stateMachine;
        }

        public override void Update()
        {
            Character.ApplyHorizontalMovement();
        }
    }
}
