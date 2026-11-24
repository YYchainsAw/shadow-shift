using UnityEngine;
using UnityEngine.UI;

namespace ShadowShift
{
    [DisallowMultipleComponent]
    public sealed class StudioDemoFeedback : MonoBehaviour
    {
        private const float SignalDuration = 1.4f;
        private const float LowerFloorHintHeight = 1.2f;
        private const float WalkwayClearance = 0.15f;
        private const float ShiftPromptClearance = 0.55f;
        private const float FirstWallClearance = 0.05f;
        private static readonly Color Green = new Color32(142, 247, 182, 255);
        private static readonly Color Violet = new Color32(228, 176, 255, 255);
        private static readonly Color Cyan = new Color32(163, 239, 245, 255);
        private static readonly Color Warning = new Color32(255, 166, 137, 255);

        [SerializeField] private LevelSession level;
        [SerializeField] private PlayerCharacter player;
        [SerializeField] private BoxCollider2D firstBlueWall;
        [SerializeField] private PhysicalSwitch fanSwitch;
        [SerializeField] private ShadowWalkwaySwitch walkwaySwitch;
        [SerializeField] private BoxCollider2D switchLanding;
        [SerializeField] private ShadowUpdraft updraft;
        [SerializeField] private BoxCollider2D fanLanding;
        [SerializeField] private BoxCollider2D upperWalkway;
        [SerializeField] private BoxCollider2D bottomGround;
        [SerializeField] private BoxCollider2D windRetryLedge;
        [SerializeField] private GameObject windJumpMarker;
        [SerializeField] private GameObject windEntryMarker;
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
        [SerializeField] private RectTransform shiftPrompt;

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
            OpeningJump,
            OpeningFindLight,
            OpeningShift,
            OpeningCrossWall,
            OpeningReturnPhysical,
            Fan,
            ReturnShift,
            ReturnCrossWall,
            ReturnPhysical,
            ReturnDrop,
            ReturnLightSwitch,
            ReturnShadowSwitch,
            ReturnPressSwitch,
            FanGap,
            Updraft,
            WindRise,
            WindCross,
            WindRetry,
            Impact,
            Charge,
            LampHandoff,
            ShadowExit,
            Exit,
            MissedLanding,
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

            ShiftRejection rejection = level.ActiveShiftRejection;
            Bounds body = player.Collider.bounds;
            Bounds wall = firstBlueWall.bounds;
            bool clearedWall = fanSwitch.IsPressed
                ? !walkwayOn && body.max.x <= wall.min.x - FirstWallClearance
                : body.min.x >= wall.max.x + FirstWallClearance;
            bool canShift = playing && (!level.IsShadow || clearedWall || rejection == ShiftRejection.Obstructed)
                && level.CanShift;
            bool showShift = canShift && !level.IsShadow;
            SetHint(ChooseHint(canShift ? ShiftRejection.None : rejection, canShift, clearedWall));
            if (shiftPrompt.gameObject.activeSelf != showShift)
                shiftPrompt.gameObject.SetActive(showShift);
            if (showShift)
                PositionMarker(shiftPrompt, new Vector3(body.center.x, body.max.y + ShiftPromptClearance, body.center.z));

            bool showWindRoute = playing && walkwayOn && !reachedWalkway && !floor.IsBroken;
            windJumpMarker.SetActive(showWindRoute);
            windEntryMarker.SetActive(showWindRoute && reachedFanLanding);
            UpdateRouteMarkers();

            float elapsed = playing ? Time.deltaTime : 0f;
            UpdateSignal(fanSignal, fanSwitch.IsPressed, ref fanWasOn, ref fanSignalTime,
                fanSwitch.transform.position, fanTarget.position, Green, elapsed);
            UpdateSignal(walkwaySignal, walkwayOn, ref walkwayWasOn, ref walkwaySignalTime,
                walkwaySwitch.transform.position, upperWalkway.bounds.center, Violet, elapsed);
        }

        private bool IsStandingOn(BoxCollider2D platform)
        {
            if (platform == null || !platform.enabled)
                return false;

            Bounds bounds = platform.bounds;
            Vector2 foot = player.FootPosition;
            return foot.x >= bounds.min.x && foot.x <= bounds.max.x
                && Mathf.Abs(foot.y - bounds.max.y) < 0.08f && player.IsGrounded();
        }

        private Hint ChooseHint(ShiftRejection rejection, bool canShift, bool clearedWall)
        {
            if (level.StateMachine.CurrentState == level.CompletedState)
                return Hint.Complete;
            if (level.StateMachine.CurrentState == level.FailedState)
                return Hint.Failed;
            if (level.StateMachine.CurrentState == level.PausedState)
                return Hint.Paused;

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
            if (IsOnLowerFloor())
                return Hint.MissedLanding;
            if (reachedWalkway)
                return Hint.Impact;
            if (walkwaySwitch.IsPressed)
                return ChooseWindHint();
            return fanSwitch.IsPressed ? ChooseReturnHint(canShift, clearedWall) : ChooseOpeningHint(canShift, clearedWall);
        }

        private Hint ChooseOpeningHint(bool canShift, bool clearedFirstWall)
        {
            if (level.IsShadow)
                return clearedFirstWall && canShift ? Hint.OpeningReturnPhysical : Hint.OpeningCrossWall;
            if (clearedFirstWall)
                return Hint.Fan;
            if (canShift)
                return Hint.OpeningShift;
            return player.IsGrounded() ? Hint.OpeningJump : Hint.OpeningFindLight;
        }

        private Hint ChooseReturnHint(bool canShift, bool clearedWall)
        {
            if (IsStandingOn(switchLanding))
            {
                if (level.IsShadow)
                    return Hint.ReturnPressSwitch;
                return canShift ? Hint.ReturnShadowSwitch : Hint.ReturnLightSwitch;
            }

            if (level.IsShadow)
                return clearedWall && canShift ? Hint.ReturnPhysical : Hint.ReturnCrossWall;
            return clearedWall ? Hint.ReturnDrop : Hint.ReturnShift;
        }

        private Hint ChooseWindHint()
        {
            if (IsStandingOn(windRetryLedge))
                return Hint.WindRetry;
            if (level.IsShadow && player.FootPosition.y >= upperWalkway.bounds.max.y + WalkwayClearance)
                return Hint.WindCross;
            if (player.StateMachine.CurrentState == player.RiseState && updraft.Contains(player.FootPosition))
                return Hint.WindRise;
            return reachedFanLanding ? Hint.Updraft : Hint.FanGap;
        }

        private bool IsOnLowerFloor()
        {
            if (bottomGround == null)
                return false;

            Bounds ground = bottomGround.bounds;
            Vector2 foot = player.FootPosition;
            return foot.x >= ground.min.x && foot.x <= ground.max.x
                && foot.y >= ground.max.y - 0.08f && foot.y <= ground.max.y + LowerFloorHintHeight
                && player.IsGrounded();
        }

        private void SetHint(Hint next)
        {
            bool showStability = next is Hint.Unstable or Hint.UnstableObstructed or Hint.Paused;
            int stabilityTenths = showStability ? Mathf.CeilToInt(level.ShadowTimeRemaining.GetValueOrDefault() * 10f) : 0;
            if (currentHint == next && displayedStabilityTenths == stabilityTenths)
                return;

            currentHint = next;
            displayedStabilityTenths = stabilityTenths;
            bool showControls = next == Hint.OpeningJump;
            controls.gameObject.SetActive(showControls);
            hintPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showControls ? 189f : 112f);
            (string title, string body, Color color) = next switch
            {
                Hint.OpeningJump => ("Jump into the light", "Move to the right edge. Hold D and press W to jump toward the blue wall.", Green),
                Hint.OpeningFindLight => ("Find the light", "Keep moving right. Hold right mouse and aim the spotlight at your feet.", Green),
                Hint.OpeningShift => ("Shift to Shadow", "Your feet are lit. Press Left Shift and keep moving right.", Green),
                Hint.OpeningCrossWall => ("Cross the blue wall", "Keep your feet in the light and move right through the wall.", Green),
                Hint.OpeningReturnPhysical => ("Return to Physical", "You're clear of the wall. Press Left Shift, then land on the green switch.", Green),
                Hint.Fan => ("Start the fan", "Step onto the green switch as Physical.", Green),
                Hint.ReturnShift => ("Turn back as Shadow", "When Shift appears, press Left Shift. Hold A to go back through the blue wall.", Violet),
                Hint.ReturnCrossWall => ("Cross back left", "Hold A and keep your feet in the light until you're clear of the blue wall.", Violet),
                Hint.ReturnPhysical => ("Return to Physical", "You're clear of the wall. Press Left Shift to drop faster, then keep moving left.", Violet),
                Hint.ReturnDrop => ("Drop to the switch", "Keep moving left as Physical. Land on the marked ledge and stop near the violet switch.", Violet),
                Hint.ReturnLightSwitch => ("Light the violet switch", "Hold right mouse and aim at your feet. Slide the lamp left if needed.", Violet),
                Hint.ReturnShadowSwitch => ("Shift at the switch", "Press Left Shift and step onto the violet switch to open the upper walkway.", Violet),
                Hint.ReturnPressSwitch => ("Press the violet switch", "Stay in the light and step onto the violet switch as Shadow.", Violet),
                Hint.FanGap => ("Cross the fan gap", "Switch to Physical to pass the violet wall. Jump right across the gap to the arrow.", Violet),
                Hint.Updraft => ("Ride the updraft", "Slide the lamp over the diamond and aim at it. Jump left from the arrow, then press Shift.", Violet),
                Hint.WindRise => ("Rise above the walkway", "Release A and rise in the light. Wait until your feet clear the violet walkway.", Violet),
                Hint.WindCross => ("Land on the walkway", "Move left onto the violet walkway. Stop near the right end where the beam still reaches.", Violet),
                Hint.WindRetry => ("Try the jump again", "Switch to Physical. From the middle of this ledge, jump right to the arrow.", Violet),
                Hint.Impact => ("Break the cracked floor", "Move the lamp with you to the left end. Shift to Physical and steer toward the crack.", Violet),
                Hint.Charge => ("Charge the battery", "Slide the lamp over the opening and aim at the battery.", Cyan),
                Hint.LampHandoff => ("Lamp 2 ready", "Follow the lower floor right to take control of lamp 2.", Cyan),
                Hint.ShadowExit => ("Controlling lamp 2", "Drag lamp 2 left to light your feet. Shift to Shadow, then jump through the blue wall.", Cyan),
                Hint.Exit => ("Reach the exit", "Head right and jump up the steps to the lit door.", Cyan),
                Hint.MissedLanding => ("Missed the landing", "Press R to restart and stay on the upper route until the floor breaks.", Warning),
                Hint.ShiftNeedsLight => ("Shift needs light", "Hold right mouse to aim at your feet, then press Shift.", Warning),
                Hint.ShiftObstructed => ("Shift blocked", "Move clear of the obstacle and press Shift again.", Warning),
                Hint.Unstable => ("Shadow unstable", "Hold right mouse to aim at your feet and regain control.", Warning),
                Hint.UnstableObstructed => ("Shadow unstable", "Obstacle blocks the shift. Relight your feet, then move clear.", Warning),
                Hint.Paused => ("Paused", "Press Esc to resume.", Cyan),
                Hint.Failed => (level.ShadowTimeRemaining is <= 0f ? "Shadow faded" : "Try again",
                    level.CanRetryAtLamp ? "Returning to lamp 2." : "Restarting the room.", Warning),
                _ => ("Room complete", "Press R to play again.", Cyan)
            };
            hintTitle.text = stabilityTenths > 0 ? $"{title} · {stabilityTenths / 10f:0.0}s" : title;
            hintBody.text = body;
            accent.color = color;
        }

        private void UpdateRouteMarkers()
        {
            bool charged = battery.IsCharged;
            bool waitingForHandoff = charged && level.HasLampHandoff && !level.ControlsNextSpotlight;
            bool showVioletSwitch = fanSwitch.IsPressed && !walkwaySwitch.IsPressed && !floor.IsBroken;
            string status = showVioletSwitch ? "Violet switch" : waitingForHandoff ? "Lamp 2 ready" : "Needs battery power";
            if (level.IsExitUnlocked)
                status = level.HasLampHandoff ? "Enter as Shadow" : "Exit open";
            if (exitStatus.text != status)
            {
                exitStatus.text = status;
                exitStatus.color = showVioletSwitch ? Violet : charged ? Cyan : new Color32(217, 214, 239, 255);
            }

            Vector3 markerPosition = showVioletSwitch ? walkwaySwitch.transform.position + Vector3.down * 1.15f
                : waitingForHandoff ? level.LampHandoffPosition + Vector3.up * 1.1f : exitTarget.position + Vector3.up * 1.9f;
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
