using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ShadowUpdraft : MonoBehaviour
    {
        private const float LandingSurfaceTolerance = 0.03f;

        [SerializeField, Min(0.01f)] private float riseSpeed = 4.4f;
        [SerializeField, Min(0.01f)] private float acceleration = 28f;
        [SerializeField] private GameObject activeVisual;
        [SerializeField, Tooltip("Optional. Lift ends over the top of this platform so Shadow can land.")]
        private BoxCollider2D landingPlatform;

        private BoxCollider2D area;

        public bool IsActive { get; private set; }
        internal float RiseSpeed => riseSpeed;
        internal float Acceleration => acceleration;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            area = GetComponent<BoxCollider2D>();
            SetActive(false);
        }

        internal void SetActive(bool active)
        {
            IsActive = active;
            if (activeVisual != null)
                activeVisual.SetActive(active);
        }

        internal bool Contains(Vector2 footPosition)
        {
            if (!IsActive || !isActiveAndEnabled || !area.enabled || !area.OverlapPoint(footPosition))
                return false;
            if (landingPlatform == null || !landingPlatform.enabled)
                return true;

            Bounds landing = landingPlatform.bounds;
            return footPosition.y < landing.max.y - LandingSurfaceTolerance
                || footPosition.x < landing.min.x || footPosition.x > landing.max.x;
        }
    }
}
