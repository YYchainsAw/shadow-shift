namespace ShadowShift
{
    internal sealed class PhysicalFormState : CharacterFormState
    {
        public PhysicalFormState(CharacterFormController form) : base(form, "Physical") { }

        public override void Enter()
        {
            Form.ApplyCollisionRules(false);
            Form.ClearStability();
        }
    }
}
