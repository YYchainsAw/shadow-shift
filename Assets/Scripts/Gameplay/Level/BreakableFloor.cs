using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class BreakableFloor : MonoBehaviour
    {
        private const float TopContactNormal = 0.65f;
        private const float SurfaceTolerance = 0.05f;

        [SerializeField] private Collider2D intactCollider;
        [SerializeField] private SpriteRenderer intactVisual;
        [SerializeField] private GameObject brokenLeft;
        [SerializeField] private GameObject brokenRight;
        [SerializeField, Min(0.01f)] private float minimumImpactSpeed = 12f;

        private Collider2D descendingCharacter;
        private bool landedHard;

        public bool IsBroken { get; private set; }

        private void Reset()
        {
            intactCollider = GetComponent<BoxCollider2D>();
            intactVisual = GetComponent<SpriteRenderer>();
        }

        private void Awake()
        {
            if (intactCollider == null)
                intactCollider = GetComponent<BoxCollider2D>();

            ResetFloor();
        }

        internal bool TryBreak(PlayerCharacter character, bool isShadow)
        {
            if (IsBroken || !isActiveAndEnabled || !intactCollider.enabled)
                return false;

            if (landedHard)
            {
                landedHard = false;
                descendingCharacter = null;
                SetBroken(true);
                return true;
            }

            float verticalSpeed = character.Body.linearVelocityY;
            float minimumFootY = intactCollider.bounds.max.y - SurfaceTolerance;
            Collider2D characterCollider = character.Collider;

            // Discrete physics can report the contact one step after the feet cross the surface.
            if (descendingCharacter == characterCollider)
                minimumFootY += verticalSpeed * Time.fixedDeltaTime;

            bool canImpact = !isShadow && verticalSpeed <= -minimumImpactSpeed
                && character.FootPosition.y >= minimumFootY;
            descendingCharacter = canImpact ? characterCollider : null;
            return false;
        }

        internal void ResetFloor()
        {
            landedHard = false;
            descendingCharacter = null;
            SetBroken(false);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CheckImpact(collision);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            CheckImpact(collision);
        }

        private void CheckImpact(Collision2D collision)
        {
            if (!isActiveAndEnabled || descendingCharacter == null || collision.collider != descendingCharacter
                || collision.otherCollider != intactCollider)
                return;

            for (int index = 0; index < collision.contactCount; index++)
            {
                // The normal on the incoming character points down when its feet hit the top.
                if (collision.GetContact(index).normal.y <= -TopContactNormal)
                {
                    landedHard = true;
                    return;
                }
            }
        }

        private void SetBroken(bool broken)
        {
            IsBroken = broken;
            intactCollider.enabled = !broken;
            if (intactVisual != null)
                intactVisual.enabled = !broken;
            if (brokenLeft != null)
                brokenLeft.SetActive(broken);
            if (brokenRight != null)
                brokenRight.SetActive(broken);
        }
    }
}
