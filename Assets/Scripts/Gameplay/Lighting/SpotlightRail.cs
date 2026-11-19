using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class SpotlightRail : MonoBehaviour
    {
        private const float BeamEdgeTolerance = 0.00001f;

        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform leftEndpoint;
        [SerializeField] private Transform rightEndpoint;
        [SerializeField] private Shader beamShader;
        [SerializeField] private Color beamTint = new Color(1f, 0.78f, 0.39f, 0.28f);
        [SerializeField, Min(0.01f)] private float beamLength = 9f;
        [SerializeField, Range(1f, 89f)] private float beamHalfAngle = 26f;

        private float minX;
        private float maxX;
        private float beamSlope;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector2? previousPointerPosition;
        private Mesh beamMesh;
        private Material beamMaterial;

        public Vector3 WorldPosition => transform.position;
        public float BeamLength { get; private set; }
        public float BeamHalfWidth => BeamLength * beamSlope;

        private void Awake()
        {
            minX = Mathf.Min(leftEndpoint.position.x, rightEndpoint.position.x);
            maxX = Mathf.Max(leftEndpoint.position.x, rightEndpoint.position.x);
            BeamLength = Mathf.Max(0f, beamLength);
            beamSlope = Mathf.Tan(Mathf.Clamp(beamHalfAngle, 0f, 89f) * Mathf.Deg2Rad);

            initialPosition = WorldPosition;
            initialPosition.x = Mathf.Clamp(initialPosition.x, minX, maxX);
            initialRotation = transform.rotation;
            ResetPosition();
            CreateBeam();
        }

        private void OnDestroy()
        {
            Destroy(beamMesh);
            Destroy(beamMaterial);
        }

        private void CreateBeam()
        {
            GameObject beam = new GameObject("Light Footprint", typeof(MeshFilter), typeof(MeshRenderer));
            beam.transform.SetParent(transform, false);

            beamMesh = new Mesh { name = "Spotlight Cone" };
            beamMesh.vertices = new[]
            {
                Vector3.zero,
                new Vector3(BeamHalfWidth, -BeamLength, 0f),
                new Vector3(-BeamHalfWidth, -BeamLength, 0f)
            };
            beamMesh.triangles = new[] { 0, 1, 2 };
            beam.GetComponent<MeshFilter>().sharedMesh = beamMesh;

            beamMaterial = new Material(beamShader) { color = beamTint };
            MeshRenderer renderer = beam.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = beamMaterial;
            renderer.sortingOrder = 3;
        }

        public void BeginControl(Vector2 pointerScreenPosition)
        {
            previousPointerPosition = pointerScreenPosition;
        }

        public void MovePointer(Vector2 pointerScreenPosition)
        {
            Vector2? previous = previousPointerPosition;
            previousPointerPosition = pointerScreenPosition;
            if (!previous.HasValue || previous.Value == pointerScreenPosition)
                return;

            if (!TryProjectPointer(previous.Value, out Vector2 previousWorld)
                || !TryProjectPointer(pointerScreenPosition, out Vector2 currentWorld))
                return;

            // Clamp each delta so moving back from an end stop works immediately.
            Vector3 position = WorldPosition;
            position.x = Mathf.Clamp(position.x + currentWorld.x - previousWorld.x, minX, maxX);
            transform.position = position;
        }

        public void AimPointer(Vector2 pointerScreenPosition)
        {
            if (!TryProjectPointer(pointerScreenPosition, out Vector2 pointerWorld))
                return;

            Vector2 direction = pointerWorld - (Vector2)WorldPosition;
            if (direction.sqrMagnitude <= BeamEdgeTolerance * BeamEdgeTolerance)
                return;

            float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public bool Contains(Vector2 footPosition)
        {
            Vector3 local = transform.InverseTransformPoint(new Vector3(footPosition.x, footPosition.y, WorldPosition.z));
            float depth = -local.y;
            return depth >= -BeamEdgeTolerance && depth <= BeamLength + BeamEdgeTolerance
                && Mathf.Abs(local.x) <= depth * beamSlope + BeamEdgeTolerance;
        }

        public void ResetPosition()
        {
            transform.SetPositionAndRotation(initialPosition, initialRotation);
            previousPointerPosition = null;
        }

        private bool TryProjectPointer(Vector2 screenPosition, out Vector2 worldPosition)
        {
            Ray ray = viewCamera.ScreenPointToRay(screenPosition);
            Plane lightPlane = new Plane(Vector3.forward, WorldPosition);
            if (lightPlane.Raycast(ray, out float distance))
            {
                worldPosition = ray.GetPoint(distance);
                return true;
            }

            worldPosition = default;
            return false;
        }
    }
}
