using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator), typeof(SpriteRenderer))]
    public sealed class PlayerAnimation : MonoBehaviour
    {
        private static readonly int MotionState = Animator.StringToHash("MotionState");

        [SerializeField] private AnimatorOverrideController shadowController;

        private Animator animator;
        private RuntimeAnimatorController physicalController;
        private SpriteRenderer spriteRenderer;
        private PlayerCharacter character;

        internal bool HasShadowAnimations => isActiveAndEnabled && shadowController != null;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            physicalController = animator.runtimeAnimatorController;
            spriteRenderer = GetComponent<SpriteRenderer>();
            character = GetComponentInParent<PlayerCharacter>();
            if (character == null || physicalController == null)
            {
                Debug.LogError("Player Animation needs a parent Player Character and an Animator Controller.", this);
                enabled = false;
            }
        }

        internal void SetShadowForm(bool isShadow)
        {
            RuntimeAnimatorController controller = isShadow ? shadowController : physicalController;
            if (animator.runtimeAnimatorController != controller)
                animator.runtimeAnimatorController = controller;
        }

        private void Update()
        {
            int motion = character.StateMachine.CurrentState switch
            {
                CharacterMoveState => 1,
                CharacterJumpState or CharacterRiseState => 2,
                CharacterFallState => 3,
                _ => 0
            };

            animator.SetInteger(MotionState, motion);

            float moveInput = character.MoveInput;
            if (moveInput != 0f)
                spriteRenderer.flipX = moveInput < 0f;
        }
    }
}
