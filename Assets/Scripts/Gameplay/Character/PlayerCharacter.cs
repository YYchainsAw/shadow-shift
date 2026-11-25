using UnityEngine;

namespace ShadowShift
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerCharacter : MonoBehaviour
    {
        private const float GroundDistance = 0.025f;

        [Header("Horizontal Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 4.5f;
        [SerializeField, Min(0.01f)] private float groundAcceleration = 45f;
        [SerializeField, Min(0.01f)] private float groundDeceleration = 36f;
        [SerializeField, Min(0.01f)] private float airAcceleration = 22f;
        [SerializeField, Min(0.01f)] private float airDeceleration = 8f;

        [Header("Jump and Fall")]
        [SerializeField, Min(0.01f)] private float jumpSpeed = 5.7f;
        [SerializeField, Min(1f)] private float fallGravityMultiplier = 1.4f;
        [SerializeField, Range(0.01f, 1f)] private float shadowFallGravityMultiplier = 0.7f;
        [SerializeField, Min(0.01f)] private float shadowMaxFallSpeed = 4.5f;
        [SerializeField, Min(0f), Tooltip("Seconds after leaving a ledge during which a jump still starts.")]
        private float coyoteTime = 0.1f;
        [SerializeField, Min(0f), Tooltip("Seconds a jump press is remembered before landing.")]
        private float jumpBufferTime = 0.12f;

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        private BoxCollider2D bodyCollider;
        private ContactFilter2D groundFilter;
        private float moveInput;
        private float jumpBufferRemaining;
        private float coyoteTimeRemaining;
        private bool isShadow;
        private bool groundCacheValid;
        private bool groundCacheResult;
        private bool groundCacheInFixedStep;
        private float groundCacheTime;
        private ShadowUpdraft updraft;
        private SpotlightRail updraftLight;
        private SpotlightRail additionalUpdraftLight;

        public StateMachine<CharacterState> StateMachine { get; } = new();
        public CharacterIdleState IdleState { get; private set; }
        public CharacterMoveState MoveState { get; private set; }
        public CharacterJumpState JumpState { get; private set; }
        public CharacterFallState FallState { get; private set; }
        public CharacterRiseState RiseState { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public BoxCollider2D Collider => bodyCollider;
        public bool SimulationEnabled { get; set; } = true;
        public string StateName => StateMachine.CurrentState.Name;

        internal float MoveInput => moveInput;
        internal bool JumpRequested => jumpBufferRemaining > 0f;
        internal bool CanCoyoteJump => coyoteTimeRemaining > 0f;

        public Vector2 FootPosition
        {
            get
            {
                Vector2 localFoot = bodyCollider.offset + Vector2.down * (bodyCollider.size.y * 0.5f);
                return Body.position + (Vector2)transform.TransformVector(localFoot);
            }
        }

        private void Awake()
        {
            Body = GetComponent<Rigidbody2D>();
            bodyCollider = GetComponent<BoxCollider2D>();
            groundFilter = new ContactFilter2D { useTriggers = false };
            IdleState = new CharacterIdleState(this, StateMachine);
            MoveState = new CharacterMoveState(this, StateMachine);
            JumpState = new CharacterJumpState(this, StateMachine);
            FallState = new CharacterFallState(this, StateMachine);
            RiseState = new CharacterRiseState(this, StateMachine);
            StateMachine.Initialize(IdleState);
        }

        public void SetMoveInput(float horizontal)
        {
            moveInput = Mathf.Clamp(horizontal, -1f, 1f);
        }

        public void RequestJump()
        {
            // Buffered: the press is kept briefly so a jump just before landing still starts.
            if (SimulationEnabled)
                jumpBufferRemaining = Mathf.Max(jumpBufferTime, Time.fixedDeltaTime);
        }

        public void ClearInput()
        {
            moveInput = 0f;
            jumpBufferRemaining = 0f;
        }

        public void StopSteering()
        {
            ClearInput();
            Body.linearVelocityX = 0f;
        }

        public void ResetMotion(Vector2 spawnPosition)
        {
            ClearInput();
            SetUpdraft(null, null);
            Body.position = spawnPosition;
            InvalidateGroundCache();
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            coyoteTimeRemaining = 0f;
            Body.WakeUp();
            StateMachine.ChangeState(IdleState);
        }

        internal void SetUpdraft(ShadowUpdraft field, SpotlightRail light, SpotlightRail additionalLight = null)
        {
            updraft = field;
            updraftLight = light;
            additionalUpdraftLight = additionalLight;
        }

        internal void SetShadowForm(bool shadow)
        {
            if (isShadow == shadow)
                return;

            isShadow = shadow;
            InvalidateGroundCache(); // Collision rules differ per form.
            if (StateMachine.CurrentState == FallState)
                FallState.ApplyFormPhysics();
        }

        private void FixedUpdate()
        {
            if (!SimulationEnabled)
                return;

            if (StateMachine.CurrentState != RiseState && CanUseUpdraft())
                StateMachine.ChangeState(RiseState);

            coyoteTimeRemaining = StateMachine.CurrentState is CharacterGroundedState && IsGrounded()
                ? coyoteTime
                : Mathf.Max(0f, coyoteTimeRemaining - Time.fixedDeltaTime);

            StateMachine.CurrentState.Update();
            jumpBufferRemaining = Mathf.Max(0f, jumpBufferRemaining - Time.fixedDeltaTime);
        }

        internal void ApplyHorizontalMovement()
        {
            bool grounded = StateMachine.CurrentState is CharacterGroundedState;
            float acceleration = grounded ? groundAcceleration : airAcceleration;
            float deceleration = grounded ? groundDeceleration : airDeceleration;
            float rate = moveInput == 0f ? deceleration : acceleration;
            Body.linearVelocityX = Mathf.MoveTowards(
                Body.linearVelocityX, moveInput * moveSpeed, rate * Time.fixedDeltaTime);
        }

        internal void BeginJump()
        {
            Body.linearVelocityY = jumpSpeed;
            coyoteTimeRemaining = 0f;
            jumpBufferRemaining = 0f;
        }

        internal void ApplyFallPhysics(float baseGravity)
        {
            float gravity = baseGravity * (isShadow ? shadowFallGravityMultiplier : fallGravityMultiplier);
            if (isShadow)
            {
                float verticalSpeed = Body.linearVelocityY;
                if (verticalSpeed < -shadowMaxFallSpeed)
                {
                    verticalSpeed = -shadowMaxFallSpeed;
                    Body.linearVelocityY = verticalSpeed;
                }

                // Limit this physics step's gravity so it does not push the Shadow past its fall speed.
                float gravityStep = -Physics2D.gravity.y * Time.fixedDeltaTime;
                if (gravityStep > 0f)
                    gravity = Mathf.Min(gravity, (verticalSpeed + shadowMaxFallSpeed) / gravityStep);
            }

            Body.gravityScale = gravity;
        }

        internal bool TryApplyUpdraft()
        {
            if (!CanUseUpdraft())
                return false;

            float verticalSpeed = Body.linearVelocityY;
            if (verticalSpeed < updraft.RiseSpeed)
                Body.linearVelocityY = Mathf.MoveTowards(
                    verticalSpeed, updraft.RiseSpeed, updraft.Acceleration * Time.fixedDeltaTime);
            return true;
        }

        private bool CanUseUpdraft()
        {
            if (updraft == null || updraftLight == null)
                return false;

            Vector2 footPosition = FootPosition;
            return updraft.Contains(footPosition) && (updraftLight.Contains(footPosition)
                || (additionalUpdraftLight != null && additionalUpdraftLight.Contains(footPosition)));
        }

        internal bool IsGrounded()
        {
            if (Body.linearVelocityY > 0.05f)
                return false;

            // The pose only changes when physics steps, so reuse the cast within one phase of a step.
            float stepTime = Time.fixedTime;
            bool inFixedStep = Time.inFixedTimeStep;
            if (groundCacheValid && groundCacheTime == stepTime && groundCacheInFixedStep == inFixedStep)
                return groundCacheResult;

            groundCacheResult = CastForGround();
            groundCacheTime = stepTime;
            groundCacheInFixedStep = inFixedStep;
            groundCacheValid = true;
            return groundCacheResult;
        }

        /// <summary>Call after anything that changes the ground under the character outside physics.</summary>
        internal void InvalidateGroundCache()
        {
            groundCacheValid = false;
        }

        private bool CastForGround()
        {
            // Query the current pose after a restart or a form's collision change.
            int count = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, GroundDistance);
            float footY = FootPosition.y;
            for (int index = 0; index < count; index++)
            {
                RaycastHit2D hit = groundHits[index];
                if (hit.normal.y >= 0.5f && hit.point.y <= footY + GroundDistance
                    && !Physics2D.GetIgnoreCollision(bodyCollider, hit.collider)
                    && !Physics2D.GetIgnoreLayerCollision(gameObject.layer, hit.collider.gameObject.layer))
                    return true;
            }

            return false;
        }
    }
}
