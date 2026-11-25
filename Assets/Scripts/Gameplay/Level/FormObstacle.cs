using UnityEngine;

namespace ShadowShift
{
    public enum ObstacleType
    {
        PhysicalSolid,
        ShadowBarrier,
        UniversalSolid
    }

    [DisallowMultipleComponent]
    public sealed class FormObstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleType type;

        public ObstacleType Type => type;

        public Collider2D Collider { get; private set; }

        private void Awake()
        {
            Collider = GetComponent<Collider2D>();
        }

        public bool Blocks(bool shadow)
        {
            return type switch
            {
                ObstacleType.PhysicalSolid => !shadow,
                ObstacleType.ShadowBarrier => shadow,
                _ => true
            };
        }
    }
}
