using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class UpdraftVisual : MonoBehaviour
    {
        [SerializeField] private LevelSession levelSession;
        [SerializeField, Tooltip("Arrow transforms directly under this airflow object.")]
        private Transform[] arrows = Array.Empty<Transform>();
        [SerializeField] private float minimumY = -7.1f;
        [SerializeField] private float maximumY = 7.1f;
        [SerializeField, Min(0.01f)] private float riseSpeed = 2.6f;

        private List<Arrow> arrowVisuals;
        private float distance;

        private void OnEnable()
        {
            if (arrowVisuals != null)
                return;

            arrowVisuals = new List<Arrow>(arrows.Length);
            foreach (Transform arrow in arrows)
            {
                if (arrow != null)
                    arrowVisuals.Add(new Arrow(arrow));
            }
        }

        private void LateUpdate()
        {
            if (levelSession != null && levelSession.StateMachine.CurrentState is not LevelPlayingState)
                return;

            float height = maximumY - minimumY;
            if (height <= 0f)
                return;

            distance = Mathf.Repeat(distance + riseSpeed * Time.deltaTime, height);
            float fadeHeight = height * 0.1f;
            foreach (Arrow arrow in arrowVisuals)
            {
                float heightAboveBottom = Mathf.Repeat(arrow.InitialY - minimumY + distance, height);
                float distanceToEdge = Mathf.Min(heightAboveBottom, height - heightAboveBottom);
                float opacity = Mathf.SmoothStep(0f, 1f, distanceToEdge / fadeHeight);
                arrow.SetVisual(minimumY + heightAboveBottom, opacity);
            }
        }

        private void OnDisable()
        {
            distance = 0f;
            foreach (Arrow arrow in arrowVisuals)
                arrow.SetVisual(arrow.InitialY, 1f);
        }

        private sealed class Arrow
        {
            private readonly Transform transform;
            private readonly Vector3 initialPosition;
            private readonly SpriteRenderer[] renderers;
            private readonly Color[] initialColors;

            public float InitialY => initialPosition.y;

            public Arrow(Transform transform)
            {
                this.transform = transform;
                initialPosition = transform.localPosition;
                renderers = transform.GetComponentsInChildren<SpriteRenderer>(true);
                initialColors = new Color[renderers.Length];
                for (int index = 0; index < renderers.Length; index++)
                    initialColors[index] = renderers[index].color;
            }

            public void SetVisual(float y, float opacity)
            {
                Vector3 position = initialPosition;
                position.y = y;
                transform.localPosition = position;

                for (int index = 0; index < renderers.Length; index++)
                {
                    Color color = initialColors[index];
                    color.a *= opacity;
                    renderers[index].color = color;
                }
            }
        }
    }
}
