using UnityEngine;

namespace ShadowShift
{
    internal sealed class ShadowFormState : CharacterFormState
    {
        public ShadowFormState(CharacterFormController form) : base(form, "Shadow") { }

        public override void Enter()
        {
            Form.ApplyCollisionRules(true);
            Form.ClearStability();
        }

        public override void Update()
        {
            Form.IsOutsideLight = !Form.IsLit;
            Form.TimeRemaining = Form.IsOutsideLight
                ? Mathf.Max(0f, Form.TimeRemaining - Form.DeltaTime)
                : Form.GracePeriod;
        }
    }
}
