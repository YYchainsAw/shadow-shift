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

        private readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];
        private readonly ContactPoint2D[] contacts = new ContactPoint2D[8];
        private BoxCollider2D bodyCollider;
        private ContactFilter2D groundFilter;
        private float moveInput;
        private bool jumpRequested;

        public StateMachine<CharacterState> StateMachine { get; } = new();
        public CharacterIdleState IdleState { get; private set; }
        public CharacterMoveState MoveState { get; private set; }
        public CharacterJumpState JumpState { get; private set; }
        public CharacterFallState FallState { get; private set; }
        public Rigidbody2D Body { get; private set; }
        public BoxCollider2D Collider => bodyCollider;
        public bool SimulationEnabled { get; set; } = true;
        public string StateName => StateMachine.CurrentState.Name;

        internal float MoveInput => moveInput;
        internal bool JumpRequested => jumpRequested;
        internal float FallGravityMultiplier => fallGravityMultiplier;

        public Vector2 FootPosition
        {
            get
            {
                Vector2 localFoot = bodyCollider.offset + Vector2.down * (bodyCollider.size.y * 0.5f);
                return Body.position + (Vector2)transform.TransformVector(localFoot);
            }
        }

        public bool IsMovementBlocked
        {
            get
            {
                int count = bodyCollider.GetContacts(contacts);
                for (int index = 0; index < count; index++)
                {
                    if (contacts[index].normal.x * moveInput < -0.5f)
                        return true;
                }

                return false;
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
            StateMachine.Initialize(IdleState);
        }

        public void SetMoveInput(float horizontal)
        {
            moveInput = Mathf.Clamp(horizontal, -1f, 1f);
        }

        public void RequestJump()
        {
            if (SimulationEnabled && IsGrounded())
                jumpRequested = true;
        }

        public void ClearInput()
        {
            moveInput = 0f;
            jumpRequested = false;
        }

        public void StopSteering()
        {
            ClearInput();
            Body.linearVelocityX = 0f;
        }

        public void ResetMotion(Vector2 spawnPosition)
        {
            ClearInput();
            Body.position = spawnPosition;
            Body.linearVelocity = Vector2.zero;
            Body.angularVelocity = 0f;
            Body.WakeUp();
            StateMachine.ChangeState(IdleState);
        }

        private void FixedUpdate()
        {
            if (!SimulationEnabled)
                return;

            StateMachine.CurrentState.Update();
            jumpRequested = false;
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
        }

        internal bool IsGrounded()
        {
            if (Body.linearVelocityY > 0.05f)
                return false;

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
