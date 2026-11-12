using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ShadowUpdraft : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float riseSpeed = 4.4f;
        [SerializeField, Min(0.01f)] private float acceleration = 28f;
        [SerializeField] private GameObject activeVisual;

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
            return IsActive && isActiveAndEnabled && area.enabled && area.OverlapPoint(footPosition);
        }
    }
}
