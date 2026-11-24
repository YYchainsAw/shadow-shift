using UnityEngine;
using UnityEngine.UI;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class StudioDemoFeedback : MonoBehaviour
    {
        private const float SignalDuration = 1.4f;
        private static readonly Color Green = new Color32(142, 247, 182, 255);
        private static readonly Color Violet = new Color32(228, 176, 255, 255);
        private static readonly Color Cyan = new Color32(163, 239, 245, 255);
        private static readonly Color Warning = new Color32(255, 166, 137, 255);

        [SerializeField] private LevelSession level;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private PhysicalSwitch fanSwitch;
        [SerializeField] private ShadowWalkwaySwitch walkwaySwitch;
        [SerializeField] private BoxCollider2D fanLanding;
        [SerializeField] private BoxCollider2D upperWalkway;
        [SerializeField] private BreakableFloor floor;
        [SerializeField] private LightBattery battery;
        [SerializeField] private Transform fanTarget;
        [SerializeField] private Transform exitTarget;
        [SerializeField] private SpriteRenderer fanSignal;
        [SerializeField] private SpriteRenderer walkwaySignal;
        [SerializeField] private Canvas overlay;
        [SerializeField] private RectTransform hintPanel;
        [SerializeField] private Text hintTitle;
        [SerializeField] private Text hintBody;
        [SerializeField] private Text controls;
        [SerializeField] private Image accent;
        [SerializeField] private Text exitStatus;
        [SerializeField] private RectTransform chargeTrack;
        [SerializeField] private RectTransform chargeFill;

        private Hint currentHint = Hint.None;
        private int displayedStabilityTenths;
        private bool reachedFanLanding;
        private bool reachedWalkway;
        private bool fanWasOn;
        private bool walkwayWasOn;
        private float fanSignalTime;
        private float walkwaySignalTime;

        private enum Hint
        {
            None,
            Fan,
            Switch,
            FanGap,
            Updraft,
            Impact,
            Charge,
            LampHandoff,
            ShadowExit,
            Exit,
            ShiftNeedsLight,
            ShiftObstructed,
            Unstable,
            UnstableObstructed,
            Paused,
            Failed,
            Complete
        }

        private void LateUpdate()
        {
            if (level.StateMachine.CurrentState == null)
                return;

            bool playing = level.StateMachine.CurrentState == level.PlayingState;
            bool walkwayOn = walkwaySwitch.IsPressed;
            if (!walkwayOn)
            {
                reachedFanLanding = false;
                reachedWalkway = false;
            }
            else
            {
                if (!reachedFanLanding)
                    reachedFanLanding = player.StateMachine.CurrentState == player.RiseState || IsStandingOn(fanLanding);
                if (!reachedWalkway && level.IsShadow)
                    reachedWalkway = IsStandingOn(upperWalkway);
            }

            SetHint(ChooseHint());
            UpdatePowerFeedback();

            float elapsed = playing ? Time.deltaTime : 0f;
            UpdateSignal(fanSignal, fanSwitch.IsPressed, ref fanWasOn, ref fanSignalTime,
                fanSwitch.transform.position, fanTarget.position, Green, elapsed);
            UpdateSignal(walkwaySignal, walkwayOn, ref walkwayWasOn, ref walkwaySignalTime,
                walkwaySwitch.transform.position, upperWalkway.bounds.center, Violet, elapsed);
        }

        private bool IsStandingOn(BoxCollider2D platform)
        {
            Bounds bounds = platform.bounds;
            Vector2 foot = player.FootPosition;
            return foot.x >= bounds.min.x && foot.x <= bounds.max.x
                && Mathf.Abs(foot.y - bounds.max.y) < 0.08f && player.IsGrounded();
        }

        private Hint ChooseHint()
        {
            if (level.StateMachine.CurrentState == level.CompletedState)
                return Hint.Complete;
            if (level.StateMachine.CurrentState == level.FailedState)
                return Hint.Failed;
            if (level.StateMachine.CurrentState == level.PausedState)
                return Hint.Paused;

            ShiftRejection rejection = level.ActiveShiftRejection;
            if (level.ShadowTimeRemaining.HasValue)
                return rejection == ShiftRejection.Obstructed ? Hint.UnstableObstructed : Hint.Unstable;
            if (rejection == ShiftRejection.NeedsLight)
                return Hint.ShiftNeedsLight;
            if (rejection == ShiftRejection.Obstructed)
                return Hint.ShiftObstructed;

            if (battery.IsCharged)
            {
                if (!level.HasLampHandoff)
                    return Hint.Exit;
                return level.ControlsNextSpotlight ? Hint.ShadowExit : Hint.LampHandoff;
            }
            if (floor.IsBroken)
                return Hint.Charge;
            if (reachedWalkway)
                return Hint.Impact;
            if (walkwaySwitch.IsPressed)
                return reachedFanLanding ? Hint.Updraft : Hint.FanGap;
            return fanSwitch.IsPressed ? Hint.Switch : Hint.Fan;
        }

        private void SetHint(Hint next)
        {
            bool showStability = next is Hint.Unstable or Hint.UnstableObstructed or Hint.Paused;
            int stabilityTenths = showStability ? Mathf.CeilToInt(level.ShadowTimeRemaining.GetValueOrDefault() * 10f) : 0;
            if (currentHint == next && displayedStabilityTenths == stabilityTenths)
                return;

            currentHint = next;
            displayedStabilityTenths = stabilityTenths;
            bool showControls = next == Hint.Fan;
            controls.gameObject.SetActive(showControls);
            hintPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showControls ? 189f : 112f);
            (string title, string body, Color color) = next switch
            {
                Hint.Fan => ("Start the fan", "Jump toward the blue wall. Aim at your feet and shift to Shadow to cross it. Use the green switch as Physical.", Green),
                Hint.Switch => ("Open the upper route", "Shift to Shadow and go back through the blue wall. Drop left to the violet switch.", Violet),
                Hint.FanGap => ("Cross the fan gap", "Switch to Physical to pass the violet wall. Jump right across the gap and land beyond the fan.", Violet),
                Hint.Updraft => ("Ride the updraft", "Light the wind's right edge. Jump left and shift to Shadow. Rise above the walkway, then move left.", Violet),
                Hint.Impact => ("Break the cracked floor", "At the walkway's left end, shift to Physical. Steer toward the crack's center as you fall.", Violet),
                Hint.Charge => ("Charge the battery", "Slide the lamp over the opening and aim at the battery.", Cyan),
                Hint.LampHandoff => ("Lamp 2 ready", "Follow the lower floor right to take control of lamp 2.", Cyan),
                Hint.ShadowExit => ("Controlling lamp 2", "Drag lamp 2 left to light your feet. Shift to Shadow, then jump through the blue wall.", Cyan),
                Hint.Exit => ("Reach the exit", "Head right and jump up the steps to the lit door.", Cyan),
                Hint.ShiftNeedsLight => ("Shift needs light", "Hold right mouse to aim at your feet, then press Shift.", Warning),
                Hint.ShiftObstructed => ("Shift blocked", "Move clear of the obstacle and press Shift again.", Warning),
                Hint.Unstable => ("Shadow unstable", "Hold right mouse to aim at your feet and regain control.", Warning),
                Hint.UnstableObstructed => ("Shadow unstable", "Obstacle blocks the shift. Relight your feet, then move clear.", Warning),
                Hint.Paused => ("Paused", "Press Esc to resume.", Cyan),
                Hint.Failed => (level.ShadowTimeRemaining is <= 0f ? "Shadow faded" : "Try again", "Restarting the room.", Warning),
                _ => ("Room complete", "Press R to play again.", Cyan)
            };
            hintTitle.text = stabilityTenths > 0 ? $"{title} · {stabilityTenths / 10f:0.0}s" : title;
            hintBody.text = body;
            accent.color = color;
        }

        private void UpdatePowerFeedback()
        {
            bool charged = battery.IsCharged;
            bool waitingForHandoff = charged && level.HasLampHandoff && !level.ControlsNextSpotlight;
            string status = waitingForHandoff ? "Lamp 2 ready" : "Needs battery power";
            if (level.IsExitUnlocked)
                status = level.HasLampHandoff ? "Enter as Shadow" : "Exit open";
            if (exitStatus.text != status)
            {
                exitStatus.text = status;
                exitStatus.color = charged ? Cyan : new Color32(217, 214, 239, 255);
            }

            Vector3 markerPosition = waitingForHandoff
                ? level.LampHandoffPosition + Vector3.up * 1.1f
                : exitTarget.position + Vector3.up * 1.9f;
            PositionMarker((RectTransform)exitStatus.transform.parent, markerPosition);

            bool showCharge = floor.IsBroken && !charged;
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

        private void PositionMarker(RectTransform marker, Vector3 worldPosition)
        {
            Camera camera = overlay.worldCamera;
            Vector2 screenPosition = camera.WorldToScreenPoint(worldPosition);
            var canvasRect = (RectTransform)overlay.transform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, camera, out Vector2 localPosition))
                marker.anchoredPosition = localPosition;
        }

        private static void UpdateSignal(SpriteRenderer signal, bool active, ref bool wasActive, ref float time,
            Vector3 source, Vector3 target, Color tint, float deltaTime)
        {
            if (active && !wasActive)
                time = 0f;
            wasActive = active;
            signal.enabled = active && time < SignalDuration;
            if (!signal.enabled)
                return;

            time += deltaTime;
            float progress = Mathf.Clamp01(time / SignalDuration);
            signal.transform.position = Vector3.Lerp(source + Vector3.up * 0.2f, target, Mathf.SmoothStep(0f, 1f, progress * 1.4f));
            tint.a = 1f - Mathf.InverseLerp(0.72f, 1f, progress);
            signal.color = tint;
        }
    }
}
