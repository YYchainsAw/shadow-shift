using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class CircuitWire : MonoBehaviour
    {
        private static Sprite fallbackPixelSprite;

        [SerializeField] private LevelSession levelSession;
        [SerializeField] private PhysicalSwitch physicalSwitch;
        [SerializeField] private ShadowWalkwaySwitch shadowSwitch;
        [SerializeField] private LightBattery battery;
        [SerializeField] private bool alwaysPowered;
        [SerializeField] private Sprite segmentSprite;
        [SerializeField] private Vector2[] waypoints = Array.Empty<Vector2>();
        [SerializeField] private Color unpoweredColor = new Color(0.38f, 0.42f, 0.52f, 0.36f);
        [SerializeField] private Color poweredColor = new Color(0.55f, 0.96f, 0.72f, 0.92f);
        [SerializeField, Min(0.02f)] private float thickness = 0.09f;
        [SerializeField, Min(0.1f)] private float pulseSpeed = 9f;
        [SerializeField] private int sortingOrder = 2;

        private readonly List<SpriteRenderer> renderers = new();
        private float[] cumulativeLengths;
        private float totalLength;
        private SpriteRenderer pulseRenderer;
        private float pulseDistance;
        private bool wasPowered;
        private bool built;

        public bool IsPowered
        {
            get
            {
                if (alwaysPowered)
                    return true;
                if (physicalSwitch != null)
                    return physicalSwitch.IsPressed;
                if (shadowSwitch != null)
                    return shadowSwitch.IsPressed;
                if (battery != null)
                    return battery.IsCharged;
                return false;
            }
        }

        private void Awake()
        {
            BuildGeometry();
            ApplyVisualState(IsPowered, true);
        }

        internal void Configure(
            LevelSession session,
            Vector2[] worldWaypoints,
            Color dimColor,
            Color activeColor,
            Sprite sprite = null,
            PhysicalSwitch sourcePhysical = null,
            ShadowWalkwaySwitch sourceShadow = null,
            LightBattery sourceBattery = null,
            float wireThickness = 0.09f,
            int order = 2)
        {
            levelSession = session;
            waypoints = worldWaypoints;
            unpoweredColor = dimColor;
            poweredColor = activeColor;
            if (sprite != null)
                segmentSprite = sprite;
            physicalSwitch = sourcePhysical;
            shadowSwitch = sourceShadow;
            battery = sourceBattery;
            thickness = wireThickness;
            sortingOrder = order;
        }

        private void LateUpdate()
        {
            if (!built || totalLength <= 0f)
                return;

            bool powered = IsPowered;
            if (powered != wasPowered)
            {
                pulseDistance = 0f;
                ApplyVisualState(powered, false);
            }

            if (!powered || pulseRenderer == null)
                return;

            bool playing = levelSession == null || levelSession.StateMachine.CurrentState == levelSession.PlayingState;
            if (playing)
                pulseDistance = Mathf.Repeat(pulseDistance + pulseSpeed * Time.deltaTime, totalLength);

            pulseRenderer.transform.position = SamplePointAlongWire(pulseDistance);
        }

        private void BuildGeometry()
        {
            if (built || waypoints == null || waypoints.Length < 2)
                return;

            built = true;
            Sprite sprite = segmentSprite != null ? segmentSprite : GetFallbackSprite();
            cumulativeLengths = new float[waypoints.Length];
            totalLength = 0f;

            CreateTerminalNode("Start Terminal", waypoints[0], sprite);
            for (int index = 1; index < waypoints.Length; index++)
            {
                Vector2 start = waypoints[index - 1];
                Vector2 end = waypoints[index];
                Vector2 delta = end - start;
                float length = delta.magnitude;
                totalLength += length;
                cumulativeLengths[index] = totalLength;
                if (length <= 0.001f)
                    continue;

                var segmentObject = new GameObject($"Wire Segment {index}", typeof(SpriteRenderer));
                segmentObject.transform.SetParent(transform, false);
                segmentObject.transform.position = (start + end) * 0.5f;
                segmentObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
                segmentObject.transform.localScale = new Vector3(length + thickness, thickness, 1f);

                var sr = segmentObject.GetComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = sortingOrder;
                renderers.Add(sr);
            }
            CreateTerminalNode("End Terminal", waypoints[waypoints.Length - 1], sprite);

            var pulseObject = new GameObject("Wire Pulse", typeof(SpriteRenderer));
            pulseObject.transform.SetParent(transform, false);
            pulseObject.transform.localScale = new Vector3(thickness * 2.6f, thickness * 2.6f, 1f);
            pulseRenderer = pulseObject.GetComponent<SpriteRenderer>();
            pulseRenderer.sprite = sprite;
            pulseRenderer.sortingOrder = sortingOrder + 1;
        }

        private void CreateTerminalNode(string nodeName, Vector2 position, Sprite sprite)
        {
            var node = new GameObject(nodeName, typeof(SpriteRenderer));
            node.transform.SetParent(transform, false);
            node.transform.position = position;
            float size = thickness * 2.2f;
            node.transform.localScale = new Vector3(size, size, 1f);

            var sr = node.GetComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder + 1;
            renderers.Add(sr);
        }

        private void ApplyVisualState(bool powered, bool force)
        {
            if (!force && wasPowered == powered)
                return;

            wasPowered = powered;
            Color target = powered ? poweredColor : unpoweredColor;
            for (int i = 0; i < renderers.Count; i++)
                renderers[i].color = target;

            if (pulseRenderer != null)
            {
                pulseRenderer.enabled = powered;
                Color pulseColor = Color.Lerp(poweredColor, Color.white, 0.55f);
                pulseColor.a = 0.95f;
                pulseRenderer.color = pulseColor;
            }
        }

        private Vector3 SamplePointAlongWire(float distance)
        {
            for (int index = 1; index < cumulativeLengths.Length; index++)
            {
                float previous = cumulativeLengths[index - 1];
                float current = cumulativeLengths[index];
                if (distance <= current || index == cumulativeLengths.Length - 1)
                {
                    float span = current - previous;
                    float t = span > 0.0001f ? Mathf.Clamp01((distance - previous) / span) : 0f;
                    return Vector2.Lerp(waypoints[index - 1], waypoints[index], t);
                }
            }

            return waypoints[0];
        }

        private static Sprite GetFallbackSprite()
        {
            if (fallbackPixelSprite == null)
            {
                Texture2D white = Texture2D.whiteTexture;
                fallbackPixelSprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height), new Vector2(0.5f, 0.5f), white.width);
                fallbackPixelSprite.name = "CircuitWirePixel";
            }

            return fallbackPixelSprite;
        }
    }
}
