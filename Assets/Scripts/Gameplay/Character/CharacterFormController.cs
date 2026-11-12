using System.Collections.Generic;
using UnityEngine;

namespace ShadowShift
{
    internal sealed class CharacterFormController
    {
        private const float OverlapTolerance = 0.001f;

        private readonly Collider2D characterCollider;
        private readonly IReadOnlyList<FormObstacle> obstacles;
        private readonly StateMachine<CharacterFormState> stateMachine = new();

        public CharacterFormController(Collider2D characterCollider, IReadOnlyList<FormObstacle> obstacles, float gracePeriod)
        {
            this.characterCollider = characterCollider;
            this.obstacles = obstacles;
            GracePeriod = Mathf.Max(0.01f, gracePeriod);
            PhysicalState = new PhysicalFormState(this);
            ShadowState = new ShadowFormState(this);
            stateMachine.Initialize(PhysicalState);
        }

        public PhysicalFormState PhysicalState { get; }
        public ShadowFormState ShadowState { get; }
        public string StateName => stateMachine.CurrentState.Name;
        public bool IsShadow => stateMachine.CurrentState == ShadowState;
        public bool IsOutsideLight { get; internal set; }
        public float TimeRemaining { get; internal set; }
        internal float GracePeriod { get; }
        internal bool IsLit { get; private set; }
        internal float DeltaTime { get; private set; }

        public string TryShift(bool isLit)
        {
            if (!IsShadow && !isLit)
                return "Move the light over the character before shifting.";

            bool destinationIsShadow = !IsShadow;
            if (!CanOccupy(destinationIsShadow))
                return destinationIsShadow
                    ? "Clear Shadow blockers before changing form."
                    : "Clear Physical blockers before changing form.";

            IsLit = isLit;
            stateMachine.ChangeState(destinationIsShadow ? ShadowState : PhysicalState);
            return IsShadow ? "Shadow can pass through Physical Solids." : "Returned to Physical.";
        }

        public bool UpdateStability(bool isLit, float deltaTime)
        {
            IsLit = isLit;
            DeltaTime = Mathf.Max(0f, deltaTime);
            stateMachine.CurrentState.Update();
            return IsOutsideLight && TimeRemaining <= 0f;
        }

        public void Reset()
        {
            if (IsShadow)
                stateMachine.ChangeState(PhysicalState);
            else
                PhysicalState.Enter();
        }

        private bool CanOccupy(bool shadow)
        {
            Physics2D.SyncTransforms();
            foreach (FormObstacle obstacle in obstacles)
            {
                // Both forms use the same collider. Shared blockers already constrain the current pose.
                if (!IsActiveObstacle(obstacle) || !obstacle.Blocks(shadow) || obstacle.Blocks(IsShadow))
                    continue;

                ColliderDistance2D distance = characterCollider.Distance(obstacle.Collider);
                if (distance.isOverlapped && distance.distance < -OverlapTolerance)
                    return false;
            }

            return true;
        }

        internal void ApplyCollisionRules(bool shadow)
        {
            foreach (FormObstacle obstacle in obstacles)
            {
                if (IsActiveObstacle(obstacle))
                    Physics2D.IgnoreCollision(characterCollider, obstacle.Collider, !obstacle.Blocks(shadow));
            }
        }

        internal void ClearStability()
        {
            IsOutsideLight = false;
            TimeRemaining = GracePeriod;
        }

        private bool IsActiveObstacle(FormObstacle obstacle)
        {
            return obstacle != null && obstacle.isActiveAndEnabled && obstacle.Collider != null
                && obstacle.Collider != characterCollider && obstacle.Collider.enabled
                && obstacle.Collider.gameObject.activeInHierarchy && !obstacle.Collider.isTrigger;
        }
    }
}
