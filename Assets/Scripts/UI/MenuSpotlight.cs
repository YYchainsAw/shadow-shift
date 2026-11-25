using UnityEngine;
using UnityEngine.UI;

namespace ShadowShift
{
    public sealed class MenuSpotlight : MaskableGraphic
    {
        private static readonly Color BeamColor = new Color32(123, 183, 228, 255);
        private static readonly Color FloorColor = new Color32(135, 170, 232, 255);
        private static readonly Color GradeColor = new Color32(19, 16, 39, 255);

        [SerializeField] private RectTransform lamp;

        private float progress;

        public void SetProgress(float value)
        {
            progress = Mathf.Clamp01(value);
            lamp.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(13f, -60f, progress));
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            AddBackdropGrade(mesh);
            if (lamp == null)
                return;

            Rect lampRect = lamp.rect;
            var lensCenter = new Vector3(lampRect.center.x, Mathf.Lerp(lampRect.yMin, lampRect.yMax, 0.236f));
            Vector2 origin = rectTransform.InverseTransformPoint(lamp.TransformPoint(lensCenter));
            Vector2 across = rectTransform.InverseTransformVector(lamp.TransformVector(Vector3.right));
            Vector2 forward = rectTransform.InverseTransformVector(lamp.TransformVector(Vector3.down));

            AddBeam(mesh, origin, across.normalized, forward.normalized);
            AddHomeLightPool(mesh);
        }

        private void AddBackdropGrade(VertexHelper mesh)
        {
            Rect rect = GetPixelAdjustedRect();
            Vector2 lowerLeft = rect.min;
            Vector2 upperLeft = new Vector2(rect.xMin, rect.yMax);
            Vector2 lowerMid = new Vector2(rect.xMin + rect.width * 0.74f, rect.yMin);
            Vector2 upperMid = new Vector2(lowerMid.x, rect.yMax);
            Vector2 lowerRight = new Vector2(rect.xMax, rect.yMin);
            Vector2 upperRight = rect.max;
            AddQuad(mesh, lowerLeft, upperLeft, upperMid, lowerMid,
                WithAlpha(GradeColor, 0.49f), WithAlpha(GradeColor, 0.42f),
                WithAlpha(GradeColor, 0.11f), WithAlpha(GradeColor, 0.16f));
            AddQuad(mesh, lowerMid, upperMid, upperRight, lowerRight,
                WithAlpha(GradeColor, 0.16f), WithAlpha(GradeColor, 0.11f),
                WithAlpha(GradeColor, 0.18f), WithAlpha(GradeColor, 0.23f));
        }

        private void AddBeam(VertexHelper mesh, Vector2 origin, Vector2 across, Vector2 forward)
        {
            float length = Mathf.Lerp(660f, 1490f, progress);
            float falloffStart = Mathf.Lerp(0.84f, 0.72f, progress);
            float opening = Mathf.Lerp(0.26f, 0.29f, progress);
            int firstVertex = mesh.currentVertCount;

            // Step across each cross-section from the clear outer edge to the visible core.
            for (int row = 0; row < 3; row++)
            {
                float travel = row == 0 ? 0f : row == 1 ? falloffStart : 1f;
                float distance = length * travel;
                float width = 45f + distance * opening;
                float feather = Mathf.Lerp(3f, 19f, travel);
                float alpha = row == 0 ? 0.38f : row == 1 ? 0.21f : 0f;
                Vector2 center = origin + forward * distance;
                mesh.AddVert(center - across * width, WithAlpha(BeamColor, 0f), Vector2.zero);
                mesh.AddVert(center - across * (width - feather), WithAlpha(BeamColor, alpha), Vector2.zero);
                mesh.AddVert(center + across * (width - feather), WithAlpha(BeamColor, alpha), Vector2.zero);
                mesh.AddVert(center + across * width, WithAlpha(BeamColor, 0f), Vector2.zero);
            }

            for (int row = 0; row < 2; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    int corner = firstVertex + row * 4 + column;
                    mesh.AddTriangle(corner, corner + 1, corner + 5);
                    mesh.AddTriangle(corner, corner + 5, corner + 4);
                }
            }
        }

        private void AddHomeLightPool(VertexHelper mesh)
        {
            float strength = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.35f, progress));
            if (strength <= 0f)
                return;

            Rect rect = rectTransform.rect;
            Vector2 center = rect.min + new Vector2(1524f, 204f);
            AddEllipse(mesh, center, new Vector2(250f, 25f), FloorColor, 0.25f * strength);
            AddEllipse(mesh, center + new Vector2(-22f, -2f), new Vector2(84f, 9f), GradeColor, 0.5f * strength);
        }

        private static void AddEllipse(VertexHelper mesh, Vector2 center, Vector2 radii, Color tint, float alpha)
        {
            const int segments = 32;
            int start = mesh.currentVertCount;
            mesh.AddVert(center, WithAlpha(tint, alpha), Vector2.zero);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                Vector2 edge = center + new Vector2(Mathf.Cos(angle) * radii.x, Mathf.Sin(angle) * radii.y);
                mesh.AddVert(edge, WithAlpha(tint, 0f), Vector2.zero);
                mesh.AddTriangle(start, start + i + 1, start + (i + 1) % segments + 1);
            }
        }

        private static void AddQuad(VertexHelper mesh, Vector2 a, Vector2 b, Vector2 c, Vector2 d,
            Color32 colorA, Color32 colorB, Color32 colorC, Color32 colorD)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(a, colorA, Vector2.zero);
            mesh.AddVert(b, colorB, Vector2.zero);
            mesh.AddVert(c, colorC, Vector2.zero);
            mesh.AddVert(d, colorD, Vector2.zero);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }

        private static Color32 WithAlpha(Color tint, float alpha)
        {
            tint.a = alpha;
            return tint;
        }
    }
}
