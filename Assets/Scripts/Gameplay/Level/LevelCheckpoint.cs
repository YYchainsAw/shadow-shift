using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class LevelCheckpoint : MonoBehaviour
    {
        [SerializeField] private int order;
        [SerializeField, Tooltip("Optional safe Physical respawn pose. Uses this transform when omitted.")]
        private Transform respawnPoint;

        private BoxCollider2D sensor;

        public int Order => order;
        public Transform RespawnTransform => respawnPoint != null ? respawnPoint : transform;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            sensor = GetComponent<BoxCollider2D>();
        }

        internal void Configure(int checkpointOrder, Transform customRespawnPoint = null)
        {
            order = checkpointOrder;
            respawnPoint = customRespawnPoint;
        }

        internal bool Overlaps(Collider2D characterCollider)
        {
            return isActiveAndEnabled && sensor != null && sensor.enabled
                && characterCollider.Distance(sensor).isOverlapped;
        }
    }
}
