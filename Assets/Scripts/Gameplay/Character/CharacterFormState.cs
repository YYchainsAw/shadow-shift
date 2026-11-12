namespace ShadowShift
{
    internal abstract class CharacterFormState : State
    {
        protected readonly CharacterFormController Form;

        protected CharacterFormState(CharacterFormController form, string name) : base(name)
        {
            Form = form;
        }
    }
}
