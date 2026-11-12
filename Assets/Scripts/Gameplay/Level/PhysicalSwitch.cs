using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class PhysicalSwitch : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer indicator;
        [SerializeField] private Color idleColor = new Color32(63, 146, 93, 255);
        [SerializeField] private Color pressedColor = new Color32(129, 255, 173, 255);

        private BoxCollider2D sensor;

        public bool IsPressed { get; private set; }

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            sensor = GetComponent<BoxCollider2D>();
            ResetSwitch();
        }

        internal bool TryPress(Collider2D character, bool isShadow)
        {
            if (IsPressed || isShadow || !isActiveAndEnabled || !sensor.enabled
                || !character.Distance(sensor).isOverlapped)
                return false;

            IsPressed = true;
            if (indicator != null)
                indicator.color = pressedColor;
            return true;
        }

        internal void ResetSwitch()
        {
            IsPressed = false;
            if (indicator != null)
                indicator.color = idleColor;
        }
    }
}
