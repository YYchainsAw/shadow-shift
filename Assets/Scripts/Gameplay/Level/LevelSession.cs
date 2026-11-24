using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowShift
{
    public sealed class LevelSession : MonoBehaviour
    {
        private const float ShiftRejectionDuration = 2f;

        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerCharacter character;
        [SerializeField] private SpriteRenderer characterVisual;
        [SerializeField] private SpotlightRail spotlight;
        [SerializeField, Tooltip("Optional. The second lamp takes over when the battery is charged and the player enters the handoff region.")]
        private SpotlightRail nextSpotlight;
        [SerializeField] private Collider2D lampHandoff;
        [SerializeField] private Transform spawnPoint;
        [SerializeField, Tooltip("Optional. Reaching this trigger completes the level.")]
        private Collider2D exit;
        [SerializeField, Tooltip("Optional. The fall boundary still applies without this trigger.")]
        private Collider2D hazard;
        [SerializeField] private FormObstacle[] obstacles;
        [SerializeField] private PhysicalSwitch fanSwitch;
        [SerializeField] private ShadowUpdraft updraft;
        [SerializeField] private ShadowWalkwaySwitch shadowSwitch;
        [SerializeField] private BreakableFloor breakableFloor;
        [SerializeField] private LightBattery battery;
        [SerializeField, Min(0.01f)] private float shadowGracePeriod = 1f;
        [SerializeField] private bool followCharacter = true;
        [SerializeField] private float cameraLookAhead = 2f;
        [SerializeField] private float cameraMinX = -3f;
        [SerializeField] private float cameraMaxX = 29f;
        [SerializeField] private float fallBoundary = -3f;

        [Header("Sound Effects")]
        [SerializeField] private AudioSource soundEffects;
        [SerializeField] private AudioClip shiftSound;
        [SerializeField] private AudioClip floorBreakSound;
        [SerializeField] private AudioClip batteryReadySound;
        [SerializeField] private AudioClip lampHandoffSound;
        [SerializeField] private AudioClip roomCompleteSound;

        private CharacterFormController form;
        private PlayerAnimation characterAnimation;
        private SpotlightInput spotlightInput;
        private Vector2 suspendedVelocity;
        private float initialCameraSize;
        private ShiftRejection shiftRejection;
        private float shiftRejectionTimeRemaining;

        public StateMachine<LevelState> StateMachine { get; } = new();
        public LevelPlayingState PlayingState { get; private set; }
        public LevelPausedState PausedState { get; private set; }
        public LevelFailedState FailedState { get; private set; }
        public LevelCompletedState CompletedState { get; private set; }
        public string StateName => StateMachine.CurrentState.Name;
        internal bool IsShadow => form != null && form.IsShadow;
        internal bool HasLampHandoff => battery != null && nextSpotlight != null && lampHandoff != null;
        internal bool ControlsNextSpotlight => HasLampHandoff && spotlightInput?.ControlledSpotlight == nextSpotlight;
        internal bool IsExitUnlocked => (battery == null || battery.IsCharged) && (!HasLampHandoff || ControlsNextSpotlight);
        internal Vector3 LampHandoffPosition => lampHandoff.bounds.center;
        internal float? ShadowTimeRemaining => form != null && form.IsOutsideLight ? form.TimeRemaining : null;
        internal ShiftRejection ActiveShiftRejection => shiftRejectionTimeRemaining > 0f ? shiftRejection : ShiftRejection.None;
        internal bool PausePressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;

        private void Start()
        {
            initialCameraSize = viewCamera.orthographicSize;
            characterAnimation = characterVisual.GetComponent<PlayerAnimation>();
            form = new CharacterFormController(character.Collider, obstacles, shadowGracePeriod);
            spotlightInput = new SpotlightInput(spotlight);
            PlayingState = new LevelPlayingState(this);
            PausedState = new LevelPausedState(this);
            FailedState = new LevelFailedState(this);
            CompletedState = new LevelCompletedState(this);
            StateMachine.Initialize(PlayingState);
            RestartLevel();
        }

        private void Update()
        {
            if (Keyboard.current?.rKey.wasPressedThisFrame == true)
            {
                RestartLevel();
                return;
            }

            StateMachine.CurrentState.Update();
        }

        private void LateUpdate()
        {
            if (!followCharacter || StateMachine.CurrentState == PlayingState)
                UpdateCamera();
        }

        private void FixedUpdate()
        {
            if (form != null && StateMachine.CurrentState == PlayingState && breakableFloor != null
                && breakableFloor.TryBreak(character, form.IsShadow))
                PlaySound(floorBreakSound);
        }

        internal void UpdateGameplay()
        {
            Mouse mouse = Mouse.current;
            if (HasLampHandoff && nextSpotlight.IsPowered && !ControlsNextSpotlight && Overlaps(lampHandoff))
            {
                spotlightInput.Select(nextSpotlight, mouse);
                battery.SetExitPower(true);
                PlaySound(lampHandoffSound, 0.72f);
            }
            spotlightInput.Update(mouse);

            Vector2 footPosition = character.FootPosition;
            bool isLitByNextSpotlight = HasLampHandoff && nextSpotlight.Contains(footPosition);
            bool isLit = isLitByNextSpotlight || spotlight.Contains(footPosition);
            shiftRejectionTimeRemaining = isLit && shiftRejection == ShiftRejection.NeedsLight
                ? 0f
                : Mathf.Max(0f, shiftRejectionTimeRemaining - Time.deltaTime);

            Keyboard keyboard = Keyboard.current;
            if (keyboard?.leftShiftKey.wasPressedThisFrame == true)
            {
                shiftRejection = form.TryShift(isLit);
                shiftRejectionTimeRemaining = shiftRejection == ShiftRejection.None ? 0f : ShiftRejectionDuration;
                character.SetShadowForm(form.IsShadow);
                if (shiftRejection == ShiftRejection.None)
                    PlaySound(shiftSound, 0.75f);
            }

            bool expired = form.UpdateStability(isLit, Time.deltaTime);
            if (expired || footPosition.y < fallBoundary || Overlaps(hazard))
            {
                StateMachine.ChangeState(FailedState);
                return;
            }

            if (battery != null && (breakableFloor == null || breakableFloor.IsBroken)
                && battery.ChargeFrom(spotlight, Time.deltaTime))
            {
                if (HasLampHandoff)
                    nextSpotlight.SetPowered(true);
                battery.SetExitPower(IsExitUnlocked);
                PlaySound(batteryReadySound, 0.6f);
            }

            if (IsExitUnlocked && (!HasLampHandoff || (form.IsShadow && isLitByNextSpotlight)) && Overlaps(exit))
            {
                StateMachine.ChangeState(CompletedState);
                PlaySound(roomCompleteSound, 0.9f);
                return;
            }

            UpdateUpdraft();
            if (shadowSwitch != null && shadowSwitch.TryPress(footPosition, form.IsShadow))
                form.ApplyCollisionRules(form.IsShadow);

            if (form.IsOutsideLight)
                character.StopSteering();
            else
                UpdateCharacterInput(keyboard);

            UpdateCharacterVisual();
        }

        private bool Overlaps(Collider2D trigger)
        {
            return trigger != null && character.Collider.Distance(trigger).isOverlapped;
        }

        private void PlaySound(AudioClip clip, float volumeScale = 1f)
        {
            if (soundEffects != null && clip != null)
                soundEffects.PlayOneShot(clip, volumeScale);
        }

        private void UpdateUpdraft()
        {
            if (updraft == null)
                return;

            if (fanSwitch != null && fanSwitch.TryPress(character.Collider, form.IsShadow))
                updraft.SetActive(true);

            character.SetUpdraft(form.IsShadow ? updraft : null, spotlight, HasLampHandoff ? nextSpotlight : null);
        }

        private void UpdateCharacterInput(Keyboard keyboard)
        {
            float movement = 0f;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed)
                    movement -= 1f;
                if (keyboard.dKey.isPressed)
                    movement += 1f;
                if (keyboard.wKey.wasPressedThisFrame)
                    character.RequestJump();
            }

            character.SetMoveInput(movement);
        }

        private void UpdateCamera()
        {
            if (!followCharacter)
            {
                float halfMapWidth = (cameraMaxX - cameraMinX) * 0.5f;
                viewCamera.orthographicSize = Mathf.Max(initialCameraSize, halfMapWidth / viewCamera.aspect);
                return;
            }

            float halfWidth = viewCamera.orthographicSize * viewCamera.aspect;
            float left = cameraMinX + halfWidth;
            float right = cameraMaxX - halfWidth;
            float center = left <= right
                ? Mathf.Clamp(character.transform.position.x + cameraLookAhead, left, right)
                : (cameraMinX + cameraMaxX) / 2f;
            Vector3 position = viewCamera.transform.position;
            position.x = center;
            viewCamera.transform.position = position;
        }

        private void UpdateCharacterVisual()
        {
            bool usesFormSprites = characterAnimation != null && characterAnimation.HasShadowAnimations;
            if (usesFormSprites)
                characterAnimation.SetShadowForm(form.IsShadow);

            if (form.IsOutsideLight)
                characterVisual.color = new Color32(255, 134, 118, 255);
            else if (usesFormSprites)
                characterVisual.color = Color.white;
            else
                characterVisual.color = form.IsShadow ? new Color32(133, 232, 237, 255) : new Color32(255, 240, 210, 255);
        }

        internal void RestartLevel()
        {
            ResumeSimulation();
            if (soundEffects != null)
                soundEffects.Stop();
            shiftRejectionTimeRemaining = 0f;
            character.ResetMotion(spawnPoint.position);
            Physics2D.SyncTransforms();
            if (shadowSwitch != null)
                shadowSwitch.ResetSwitch();
            if (breakableFloor != null)
                breakableFloor.ResetFloor();
            if (battery != null)
                battery.ResetCharge();
            form.Reset();
            character.SetShadowForm(form.IsShadow);
            spotlight.ResetPosition();
            if (nextSpotlight != null)
            {
                nextSpotlight.ResetPosition();
                nextSpotlight.SetPowered(false);
            }
            spotlightInput.Select(spotlight, Mouse.current);
            if (fanSwitch != null)
                fanSwitch.ResetSwitch();
            if (updraft != null)
                updraft.SetActive(false);
            UpdateCharacterVisual();
            UpdateCamera();
            StateMachine.ChangeState(PlayingState);
        }

        internal void SuspendSimulation()
        {
            character.ClearInput();
            suspendedVelocity = character.Body.linearVelocity;
            character.SimulationEnabled = false;
            character.Body.simulated = false;
            spotlightInput.Reset(Mouse.current);
        }

        internal void ResumeSimulation()
        {
            if (!character.Body.simulated)
            {
                character.Body.simulated = true;
                character.Body.linearVelocity = suspendedVelocity;
                form.ApplyCollisionRules(form.IsShadow);
            }

            character.ClearInput();
            character.SimulationEnabled = true;
            spotlightInput.Reset(Mouse.current);
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && spotlightInput != null && StateMachine.CurrentState == PlayingState)
            {
                character.StopSteering();
                spotlightInput.Reset(Mouse.current);
            }
        }
    }
}
