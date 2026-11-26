using System;
using UnityEngine;
using UnityEngine.UI;

namespace ShadowShift
{
    public enum CalloutRule
    {
        MovedRight,
        Jumped,
        AimedLight,
        EnteredShadow,
        DraggedLamp,
        ReturnedToPhysical
    }

    [Serializable]
    public struct WorldCallout
    {
        public CanvasGroup group;
        public Vector3 worldPosition;
        public CalloutRule dismissRule;
        public float thresholdX;
    }

    [DisallowMultipleComponent]
    public sealed class LevelGuidance : MonoBehaviour
    {
        private const float OverheadClearance = 0.58f;
        private static readonly Color ShiftReadyBg = new Color32(22, 36, 52, 224);
        private static readonly Color ShiftReadyText = new Color32(163, 239, 245, 255);
        private static readonly Color WarningBg = new Color32(58, 24, 28, 236);
        private static readonly Color WarningText = new Color32(255, 174, 152, 255);

        [SerializeField] private LevelSession level;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private BreakableFloor floor;
        [SerializeField] private LightBattery battery;
        [SerializeField] private SpriteRenderer handoffFloorMarker;
        [SerializeField] private Canvas overlay;
        [SerializeField] private RectTransform overheadBadge;
        [SerializeField] private Image overheadBackground;
        [SerializeField] private Text overheadLabel;
        [SerializeField] private RectTransform chargeTrack;
        [SerializeField] private RectTransform chargeFill;
        [SerializeField] private RectTransform handoffBadge;
        [SerializeField] private bool showShiftBadgeWhenLit = true;
        [SerializeField] private WorldCallout[] callouts = Array.Empty<WorldCallout>();

        private bool[] calloutCompleted;
        private float initialSpotlightX;
        private float initialSpotlightAngle;
        private Vector2 initialPlayerPosition;
        private bool recordedInitialPose;
        private string lastBadgeText = string.Empty;
        private Color lastBadgeColor;

        private void Start()
        {
            calloutCompleted = new bool[callouts != null ? callouts.Length : 0];
            CaptureInitialState();
        }

        private void LateUpdate()
        {
            if (level == null || level.StateMachine.CurrentState == null)
                return;

            if (!recordedInitialPose)
                CaptureInitialState();

            bool playing = level.StateMachine.CurrentState == level.PlayingState;
            bool paused = level.StateMachine.CurrentState == level.PausedState;
            DetectFullReset();

            UpdateOverheadBadge(playing, paused);
            UpdateBatteryAndHandoff(playing);
            UpdateCallouts(playing);
        }

        private void CaptureInitialState()
        {
            if (player == null || level == null)
                return;

            initialPlayerPosition = player.FootPosition;
            initialSpotlightX = level.PrimarySpotlightPosition.x;
            initialSpotlightAngle = level.PrimarySpotlightAngle;
            recordedInitialPose = true;
        }

        private void DetectFullReset()
        {
            if (calloutCompleted == null || calloutCompleted.Length == 0)
                return;

            // If the player restarted back at spawn in Physical with lamp reset, restore Level 1 callouts.
            if (!level.IsShadow
                && Vector2.Distance(player.FootPosition, initialPlayerPosition) < 0.35f
                && Mathf.Abs(level.PrimarySpotlightPosition.x - initialSpotlightX) < 0.1f)
            {
                for (int i = 0; i < calloutCompleted.Length; i++)
                    calloutCompleted[i] = false;
            }
        }

        private void UpdateOverheadBadge(bool playing, bool paused)
        {
            if (overheadBadge == null || overheadLabel == null)
                return;

            string text = string.Empty;
            Color bg = ShiftReadyBg;
            Color fg = ShiftReadyText;

            if ((playing || paused) && level.ShadowTimeRemaining is float remaining)
            {
                int tenths = Mathf.Max(0, Mathf.CeilToInt(remaining * 10f));
                text = $"{tenths / 10f:0.0}s";
                bg = WarningBg;
                fg = WarningText;
            }
            else if (playing && level.ActiveShiftRejection == ShiftRejection.NeedsLight)
            {
                text = "Need Light";
                bg = WarningBg;
                fg = WarningText;
            }
            else if (playing && level.ActiveShiftRejection == ShiftRejection.Obstructed)
            {
                text = "Blocked";
                bg = WarningBg;
                fg = WarningText;
            }
            else if (playing && showShiftBadgeWhenLit && !level.IsShadow && level.CanShift)
            {
                text = "Shift";
            }

            bool show = text.Length > 0;
            if (overheadBadge.gameObject.activeSelf != show)
                overheadBadge.gameObject.SetActive(show);
            if (!show)
                return;

            if (lastBadgeText != text || lastBadgeColor != fg)
            {
                lastBadgeText = text;
                lastBadgeColor = fg;
                overheadLabel.text = text;
                overheadLabel.color = fg;
                if (overheadBackground != null)
                    overheadBackground.color = bg;

                float width = Mathf.Max(68f, text.Length * 11f + 26f);
                overheadBadge.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }

            Bounds body = player.Collider.bounds;
            PositionMarker(overheadBadge, new Vector3(body.center.x, body.max.y + OverheadClearance, body.center.z));
        }

        private void UpdateBatteryAndHandoff(bool playing)
        {
            bool charged = battery != null && battery.IsCharged;
            bool floorOpen = floor == null || floor.IsBroken;

            if (chargeTrack != null)
            {
                bool showCharge = playing && battery != null && floorOpen && !charged;
                if (chargeTrack.gameObject.activeSelf != showCharge)
                    chargeTrack.gameObject.SetActive(showCharge);
                if (showCharge)
                {
                    Vector3 scale = chargeFill.localScale;
                    scale.x = battery.ChargeProgress;
                    chargeFill.localScale = scale;
                    PositionMarker(chargeTrack, battery.transform.position + Vector3.up * 0.68f);
                }
            }

            bool waitingForHandoff = charged && level.HasLampHandoff && !level.ControlsNextSpotlight;
            if (handoffFloorMarker != null)
            {
                handoffFloorMarker.gameObject.SetActive(waitingForHandoff || level.ControlsNextSpotlight);
                if (waitingForHandoff)
                {
                    Color c = new Color32(120, 240, 245, 255);
                    c.a = 0.55f + 0.35f * Mathf.Sin(Time.time * 5f);
                    handoffFloorMarker.color = c;
                }
                else if (level.ControlsNextSpotlight)
                {
                    handoffFloorMarker.color = new Color32(120, 240, 245, 190);
                }
            }

            if (handoffBadge != null)
            {
                bool showHandoff = playing && waitingForHandoff;
                if (handoffBadge.gameObject.activeSelf != showHandoff)
                    handoffBadge.gameObject.SetActive(showHandoff);
                if (showHandoff)
                    PositionMarker(handoffBadge, level.LampHandoffPosition + Vector3.up * 1.05f);
            }
        }

        private void UpdateCallouts(bool playing)
        {
            if (callouts == null || callouts.Length == 0)
                return;

            Vector2 foot = player.FootPosition;
            for (int i = 0; i < callouts.Length; i++)
            {
                WorldCallout callout = callouts[i];
                if (callout.group == null)
                    continue;

                if (!calloutCompleted[i] && playing && EvaluateDismiss(callout, foot))
                    calloutCompleted[i] = true;

                float targetAlpha = playing && !calloutCompleted[i] ? 1f : 0f;
                callout.group.alpha = Mathf.MoveTowards(callout.group.alpha, targetAlpha, Time.deltaTime * 4.5f);
                bool active = callout.group.alpha > 0.01f;
                if (callout.group.gameObject.activeSelf != active)
                    callout.group.gameObject.SetActive(active);
                if (active)
                    PositionMarker((RectTransform)callout.group.transform, callout.worldPosition);
            }
        }

        private bool EvaluateDismiss(WorldCallout callout, Vector2 foot)
        {
            return callout.dismissRule switch
            {
                CalloutRule.MovedRight => foot.x >= callout.thresholdX,
                CalloutRule.Jumped => !player.IsGrounded() && foot.x >= callout.thresholdX,
                CalloutRule.AimedLight => Mathf.Abs(Mathf.DeltaAngle(level.PrimarySpotlightAngle, initialSpotlightAngle)) > 8f,
                CalloutRule.EnteredShadow => level.IsShadow,
                CalloutRule.DraggedLamp => Mathf.Abs(level.PrimarySpotlightPosition.x - initialSpotlightX) > 1.8f,
                CalloutRule.ReturnedToPhysical => !level.IsShadow && foot.x >= callout.thresholdX,
                _ => false
            };
        }

        private void PositionMarker(RectTransform marker, Vector3 worldPosition)
        {
            if (overlay == null)
                return;

            Camera camera = overlay.worldCamera;
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
            var canvasRect = (RectTransform)overlay.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out Vector2 localPosition))
                marker.anchoredPosition = localPosition;
        }
    }
}
