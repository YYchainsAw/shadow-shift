using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class ShadowWalkwaySwitch : MonoBehaviour
    {
        [SerializeField] private Collider2D walkway;
        [SerializeField] private SpriteRenderer indicator;
        [SerializeField] private Color idleColor = new Color32(132, 94, 168, 255);
        [SerializeField] private Color pressedColor = new Color32(234, 184, 255, 255);
        [SerializeField, Range(0f, 1f)] private float inactiveOpacity = 0.38f;

        private BoxCollider2D sensor;
        private SpriteRenderer[] walkwayRenderers;
        private Color[] activeColors;

        public bool IsPressed { get; private set; }

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            sensor = GetComponent<BoxCollider2D>();
            if (walkway == null)
            {
                Debug.LogError("The shadow switch needs a walkway collider.", this);
                enabled = false;
                return;
            }

            walkwayRenderers = walkway.GetComponentsInChildren<SpriteRenderer>();
            activeColors = new Color[walkwayRenderers.Length];
            for (int i = 0; i < walkwayRenderers.Length; i++)
                activeColors[i] = walkwayRenderers[i].color;

            ResetSwitch();
        }

        internal bool TryPress(Vector2 footPosition, bool isShadow)
        {
            if (IsPressed || !isShadow || !isActiveAndEnabled || !sensor.enabled || !sensor.OverlapPoint(footPosition))
                return false;

            SetPressed(true);
            return true;
        }

        internal void ResetSwitch()
        {
            if (walkwayRenderers != null)
                SetPressed(false);
        }

        private void SetPressed(bool pressed)
        {
            IsPressed = pressed;
            walkway.enabled = pressed;
            if (indicator != null)
                indicator.color = pressed ? pressedColor : idleColor;

            for (int i = 0; i < walkwayRenderers.Length; i++)
            {
                Color color = activeColors[i];
                if (!pressed)
                    color.a *= inactiveOpacity;
                walkwayRenderers[i].color = color;
            }
        }
    }
}
