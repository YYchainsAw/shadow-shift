using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class SpotlightRail : MonoBehaviour
    {
        private const float BeamEdgeTolerance = 0.00001f;
        private const float SurfaceContactTolerance = 0.02f;

        [SerializeField] private Camera viewCamera;
        [SerializeField] private Transform leftEndpoint;
        [SerializeField] private Transform rightEndpoint;
        [SerializeField] private Shader beamShader;
        [SerializeField] private Color beamTint = new Color(1f, 0.78f, 0.39f, 0.28f);
        [SerializeField, Min(0.01f)] private float beamLength = 9f;
        [SerializeField, Range(1f, 89f)] private float beamHalfAngle = 26f;
        [SerializeField, Tooltip("Enabled rectangular colliders that cast shadows in this beam.")]
        private BoxCollider2D[] beamOccluders = Array.Empty<BoxCollider2D>();

        private float minX;
        private float maxX;
        private float beamSlope;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector2? previousPointerPosition;
        private Mesh beamMesh;
        private Material beamMaterial;
        private MeshRenderer beamRenderer;
        private List<BeamEdge> beamEdges;
        private List<float> beamBreaks;
        private List<Vector3> beamVertices;
        private List<int> beamTriangles;

        public Vector3 WorldPosition => transform.position;
        public float BeamLength { get; private set; }
        public float BeamHalfWidth => BeamLength * beamSlope;
        internal bool IsPowered { get; private set; } = true;
        private bool HasOccluders => beamOccluders != null && beamOccluders.Length > 0;

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

        private void LateUpdate()
        {
            if (IsPowered && HasOccluders && BeamLength > 0f)
                UpdateBeamMesh();
        }

        private void CreateBeam()
        {
            GameObject beam = new GameObject("Light Footprint", typeof(MeshFilter), typeof(MeshRenderer));
            beam.transform.SetParent(transform, false);

            beamMesh = new Mesh { name = "Spotlight Cone" };
            if (HasOccluders)
                beamMesh.MarkDynamic();

            beamMesh.vertices = new[]
            {
                Vector3.zero,
                new Vector3(BeamHalfWidth, -BeamLength, 0f),
                new Vector3(-BeamHalfWidth, -BeamLength, 0f)
            };
            beamMesh.triangles = new[] { 0, 1, 2 };
            beam.GetComponent<MeshFilter>().sharedMesh = beamMesh;

            beamMaterial = new Material(beamShader) { color = beamTint };
            beamRenderer = beam.GetComponent<MeshRenderer>();
            beamRenderer.sharedMaterial = beamMaterial;
            beamRenderer.sortingOrder = 3;
            beamRenderer.enabled = IsPowered;
        }

        internal void SetPowered(bool powered)
        {
            IsPowered = powered;
            if (beamRenderer != null)
                beamRenderer.enabled = powered;
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

        public bool Contains(Vector2 worldPosition)
        {
            if (!IsPowered)
                return false;

            Vector3 local = transform.InverseTransformPoint(new Vector3(worldPosition.x, worldPosition.y, WorldPosition.z));
            float depth = -local.y;
            bool inBeam = depth >= -BeamEdgeTolerance && depth <= BeamLength + BeamEdgeTolerance
                && Mathf.Abs(local.x) <= depth * beamSlope + BeamEdgeTolerance;
            if (!inBeam)
                return false;

            if (!HasOccluders)
                return true;

            if (!CollectBeamEdges())
                return false;

            if (depth <= BeamEdgeTolerance)
                return true;

            Vector2 ray = new Vector2(local.x / depth, -1f);
            // A player's feet may overlap the face of the platform they are standing on.
            float contactDepth = SurfaceContactTolerance * depth / Vector2.Distance(worldPosition, WorldPosition);
            return FindNearestEdge(ray, depth - contactDepth) == 0;
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

        private bool CollectBeamEdges()
        {
            beamEdges ??= new List<BeamEdge>();
            beamEdges.Clear();
            beamEdges.Add(new BeamEdge(new Vector2(-BeamHalfWidth, -BeamLength),
                new Vector2(BeamHalfWidth, -BeamLength)));

            Matrix4x4 worldToBeam = transform.worldToLocalMatrix;
            foreach (BoxCollider2D box in beamOccluders)
            {
                if (box == null || !box.enabled || !box.gameObject.activeInHierarchy)
                    continue;

                Rigidbody2D body = box.attachedRigidbody;
                if (body != null && !body.simulated)
                    continue;

                Matrix4x4 boxToBeam = worldToBeam * box.transform.localToWorldMatrix;
                Vector2 center = boxToBeam.MultiplyPoint3x4(box.offset);
                Vector2 halfSize = box.size * 0.5f;
                Vector2 halfX = boxToBeam.MultiplyVector(new Vector2(halfSize.x, 0f));
                Vector2 halfY = boxToBeam.MultiplyVector(new Vector2(0f, halfSize.y));
                float winding = Cross(halfX, halfY);
                if (winding == 0f)
                    continue;

                Vector2 bottomLeft = center - halfX - halfY;
                Vector2 bottomRight = center + halfX - halfY;
                Vector2 topRight = center + halfX + halfY;
                Vector2 topLeft = center - halfX + halfY;

                int previousCount = beamEdges.Count;
                AddFacingEdge(bottomLeft, bottomRight, winding);
                AddFacingEdge(bottomRight, topRight, winding);
                AddFacingEdge(topRight, topLeft, winding);
                AddFacingEdge(topLeft, bottomLeft, winding);
                if (beamEdges.Count == previousCount)
                    return false; // The source is inside or touching this box.
            }

            return true;
        }

        private void AddFacingEdge(Vector2 start, Vector2 end, float winding)
        {
            if (Cross(start, end) * winding < 0f)
                beamEdges.Add(new BeamEdge(start, end));
        }

        private int FindNearestEdge(Vector2 ray, float depth)
        {
            int nearest = 0;
            for (int index = 1; index < beamEdges.Count; index++)
            {
                BeamEdge edge = beamEdges[index];
                float denominator = Cross(ray, edge.Delta);
                if (denominator == 0f)
                    continue;

                float edgeFraction = Cross(edge.Start, ray) / denominator;
                if (edgeFraction < -BeamEdgeTolerance || edgeFraction > 1f + BeamEdgeTolerance)
                    continue;

                float hitDepth = Cross(edge.Start, edge.Delta) / denominator;
                if (hitDepth >= 0f && hitDepth < depth)
                {
                    nearest = index;
                    depth = hitDepth;
                }
            }

            return nearest;
        }

        private void UpdateBeamMesh()
        {
            beamMesh.Clear();
            if (!CollectBeamEdges())
                return;

            beamBreaks ??= new List<float>();
            beamVertices ??= new List<Vector3>();
            beamTriangles ??= new List<int>();
            beamBreaks.Clear();
            beamVertices.Clear();
            beamTriangles.Clear();
            beamBreaks.Add(-beamSlope);
            beamBreaks.Add(beamSlope);
            beamVertices.Add(Vector3.zero);

            // The nearest edge can change only where an edge starts, ends or crosses another.
            for (int index = 1; index < beamEdges.Count; index++)
            {
                BeamEdge edge = beamEdges[index];
                AddBeamBreak(edge.Start);
                AddBeamBreak(edge.Start + edge.Delta);
                for (int other = 0; other < index; other++)
                {
                    if (edge.TryIntersect(beamEdges[other], out Vector2 intersection))
                        AddBeamBreak(intersection);
                }
            }

            beamBreaks.Sort();
            for (int index = 1; index < beamBreaks.Count; index++)
            {
                float left = beamBreaks[index - 1];
                float right = beamBreaks[index];
                if (right - left <= BeamEdgeTolerance)
                    continue;

                Vector2 middleRay = new Vector2((left + right) * 0.5f, -1f);
                BeamEdge edge = beamEdges[FindNearestEdge(middleRay, BeamLength)];
                int firstVertex = beamVertices.Count;
                beamVertices.Add(PointOnBeamEdge(left, edge));
                beamVertices.Add(PointOnBeamEdge(right, edge));
                beamTriangles.Add(0);
                beamTriangles.Add(firstVertex + 1);
                beamTriangles.Add(firstVertex);
            }

            beamMesh.SetVertices(beamVertices);
            beamMesh.SetTriangles(beamTriangles, 0);
        }

        private void AddBeamBreak(Vector2 point)
        {
            float depth = -point.y;
            if (depth <= 0f || depth > BeamLength + BeamEdgeTolerance)
                return;

            float slope = point.x / depth;
            if (slope > -beamSlope && slope < beamSlope)
                beamBreaks.Add(slope);
        }

        private Vector2 PointOnBeamEdge(float slope, BeamEdge edge)
        {
            Vector2 ray = new Vector2(slope, -1f);
            float depth = Cross(edge.Start, edge.Delta) / Cross(ray, edge.Delta);
            return ray * Mathf.Clamp(depth, 0f, BeamLength);
        }

        private static float Cross(Vector2 left, Vector2 right) => left.x * right.y - left.y * right.x;

        private readonly struct BeamEdge
        {
            public BeamEdge(Vector2 start, Vector2 end)
            {
                Start = start;
                Delta = end - start;
            }

            public Vector2 Start { get; }
            public Vector2 Delta { get; }

            public bool TryIntersect(BeamEdge other, out Vector2 point)
            {
                point = default;
                float denominator = Cross(Delta, other.Delta);
                if (denominator == 0f)
                    return false;

                Vector2 difference = other.Start - Start;
                float fraction = Cross(difference, other.Delta) / denominator;
                float otherFraction = Cross(difference, Delta) / denominator;
                if (fraction <= 0f || fraction >= 1f || otherFraction <= 0f || otherFraction >= 1f)
                    return false;

                point = Start + Delta * fraction;
                return true;
            }
        }
    }
}
