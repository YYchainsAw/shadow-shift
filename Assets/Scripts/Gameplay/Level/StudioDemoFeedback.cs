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

        [SerializeField] private LevelSession level;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private PhysicalSwitch fanSwitch;
        [SerializeField] private ShadowWalkwaySwitch walkwaySwitch;
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
        private bool reachedWalkway;
        private bool fanWasOn;
        private bool walkwayWasOn;
        private bool? exitWasPowered;
        private float fanSignalTime;
        private float walkwaySignalTime;

        private enum Hint
        {
            None,
            Fan,
            Switch,
            Updraft,
            Impact,
            Charge,
            Exit,
            Complete
        }

        private void LateUpdate()
        {
            if (level.StateMachine.CurrentState == null)
                return;

            bool playing = level.StateMachine.CurrentState == level.PlayingState;
            bool walkwayOn = walkwaySwitch.IsPressed;
            if (!walkwayOn)
                reachedWalkway = false;
            else if (!reachedWalkway && level.IsShadow)
                reachedWalkway = IsOnUpperWalkway();

            SetHint(ChooseHint());
            UpdatePowerFeedback();

            float elapsed = playing ? Time.deltaTime : 0f;
            UpdateSignal(fanSignal, fanSwitch.IsPressed, ref fanWasOn, ref fanSignalTime,
                fanSwitch.transform.position, fanTarget.position, Green, elapsed);
            UpdateSignal(walkwaySignal, walkwayOn, ref walkwayWasOn, ref walkwaySignalTime,
                walkwaySwitch.transform.position, upperWalkway.bounds.center, Violet, elapsed);
        }

        private bool IsOnUpperWalkway()
        {
            Bounds bounds = upperWalkway.bounds;
            Vector2 foot = player.FootPosition;
            return foot.x >= bounds.min.x && foot.x <= bounds.max.x
                && Mathf.Abs(foot.y - bounds.max.y) < 0.08f && player.IsGrounded();
        }

        private Hint ChooseHint()
        {
            if (level.StateMachine.CurrentState == level.CompletedState)
                return Hint.Complete;
            if (battery.IsCharged)
                return Hint.Exit;
            if (floor.IsBroken)
                return Hint.Charge;
            if (reachedWalkway)
                return Hint.Impact;
            if (walkwaySwitch.IsPressed)
                return Hint.Updraft;
            return fanSwitch.IsPressed ? Hint.Switch : Hint.Fan;
        }

        private void SetHint(Hint next)
        {
            if (currentHint == next)
                return;

            currentHint = next;
            bool showControls = next == Hint.Fan;
            controls.gameObject.SetActive(showControls);
            hintPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showControls ? 189f : 112f);
            (string title, string body, Color color) = next switch
            {
                Hint.Fan => ("Start the fan", "Jump toward the blue wall. Aim at your feet and shift to Shadow to cross it. Use the green switch as Physical.", Green),
                Hint.Switch => ("Open the upper route", "Return left to the violet switch. Step on it as Shadow to power the upper walkway.", Violet),
                Hint.Updraft => ("Ride the updraft", "Cross right as Physical. Jump left into the lit wind, then shift to Shadow and rise to the violet walkway.", Violet),
                Hint.Impact => ("Break the cracked floor", "Move left along the violet walkway. Switch to Physical and drop onto the crack.", Violet),
                Hint.Charge => ("Charge the battery", "Slide the lamp over the opening and aim at the battery to power the exit.", Cyan),
                Hint.Exit => ("Reach the exit", "Head right and jump up the steps to the lit door.", Cyan),
                _ => ("Room complete", "Press R to play again.", Cyan)
            };
            hintTitle.text = title;
            hintBody.text = body;
            accent.color = color;
        }

        private void UpdatePowerFeedback()
        {
            bool powered = battery.IsCharged;
            if (exitWasPowered != powered)
            {
                exitWasPowered = powered;
                exitStatus.text = powered ? "Exit open" : "Needs battery power";
                exitStatus.color = powered ? Cyan : new Color32(217, 214, 239, 255);
            }
            PositionMarker((RectTransform)exitStatus.transform.parent, exitTarget.position + Vector3.up * 1.9f);

            bool showCharge = floor.IsBroken && !powered;
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
