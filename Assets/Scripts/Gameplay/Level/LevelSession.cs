using UnityEngine;
using UnityEngine.InputSystem;

namespace ShadowShift
{
    public sealed class LevelSession : MonoBehaviour
    {
        [SerializeField] private Camera viewCamera;
        [SerializeField] private PlayerCharacter character;
        [SerializeField] private SpriteRenderer characterVisual;
        [SerializeField] private SpotlightRail spotlight;
        [SerializeField] private Transform spawnPoint;
        [SerializeField, Tooltip("Optional. Reaching this trigger completes the level.")]
        private Collider2D exit;
        [SerializeField, Tooltip("Optional. The fall boundary still applies without this trigger.")]
        private Collider2D hazard;
        [SerializeField] private FormObstacle[] obstacles;
        [SerializeField] private PhysicalSwitch fanSwitch;
        [SerializeField] private ShadowUpdraft updraft;
        [SerializeField, Min(0.01f)] private float shadowGracePeriod = 1f;
        [SerializeField] private float cameraLookAhead = 2f;
        [SerializeField] private float cameraMinX = -3f;
        [SerializeField] private float cameraMaxX = 29f;
        [SerializeField] private float fallBoundary = -3f;
        [SerializeField, TextArea(2, 3)] private string obstacleHint =
            "Brown walls block Physical. Purple walls block Shadow. Grey blocks stop both.";

        private CharacterFormController form;
        private SpotlightInput spotlightInput;
        private Vector2 suspendedVelocity;
        private string feedback;

        public StateMachine<LevelState> StateMachine { get; } = new();
        public LevelPlayingState PlayingState { get; private set; }
        public LevelPausedState PausedState { get; private set; }
        public LevelFailedState FailedState { get; private set; }
        public LevelCompletedState CompletedState { get; private set; }
        public string StateName => StateMachine.CurrentState.Name;
        internal bool PausePressed => Keyboard.current?.escapeKey.wasPressedThisFrame == true;

        private void Start()
        {
            form = new CharacterFormController(character.Collider, obstacles, shadowGracePeriod);
            spotlightInput = new SpotlightInput(spotlight);
            PlayingState = new LevelPlayingState(this);
            PausedState = new LevelPausedState(this);
            FailedState = new LevelFailedState(this);
            CompletedState = new LevelCompletedState(this);
            StateMachine.Initialize(PlayingState);
            RestartLevel("Move with A/D, jump with W, and bring the light through the walls.");
        }

        private void Update()
        {
            if (Keyboard.current?.rKey.wasPressedThisFrame == true)
            {
                RestartLevel("Level restarted.");
                return;
            }

            StateMachine.CurrentState.Update();
        }

        private void LateUpdate()
        {
            if (StateMachine.CurrentState == PlayingState)
                UpdateCamera();
        }

        internal void UpdateGameplay()
        {
            spotlightInput.Update(Mouse.current);
            Vector2 footPosition = character.FootPosition;
            bool isLit = spotlight.Contains(footPosition);
            Keyboard keyboard = Keyboard.current;
            if (keyboard?.leftShiftKey.wasPressedThisFrame == true)
                feedback = form.TryShift(isLit);

            bool expired = form.UpdateStability(isLit, Time.deltaTime);
            if (expired || footPosition.y < fallBoundary || Overlaps(hazard))
            {
                feedback = expired ? "Shadow lost its light." : "Hazard reached or character fell.";
                StateMachine.ChangeState(FailedState);
                return;
            }

            if (Overlaps(exit))
            {
                StateMachine.ChangeState(CompletedState);
                return;
            }

            UpdateUpdraft();
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

        private void UpdateUpdraft()
        {
            if (updraft == null)
                return;

            if (fanSwitch != null && fanSwitch.TryPress(character.Collider, form.IsShadow))
                updraft.SetActive(true);

            character.SetUpdraft(form.IsShadow ? updraft : null, spotlight);
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
            characterVisual.color = form.IsOutsideLight ? new Color32(255, 134, 118, 255)
                : form.IsShadow ? new Color32(133, 232, 237, 255) : new Color32(255, 240, 210, 255);
        }

        internal void RestartLevel(string message)
        {
            ResumeSimulation();
            character.ResetMotion(spawnPoint.position);
            Physics2D.SyncTransforms();
            form.Reset();
            spotlight.ResetPosition();
            if (fanSwitch != null)
                fanSwitch.ResetSwitch();
            if (updraft != null)
                updraft.SetActive(false);
            UpdateCharacterVisual();
            UpdateCamera();
            feedback = message;
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

        internal void ShowCompletion()
        {
            feedback = "Exit reached. Press R to play again.";
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && spotlightInput != null && StateMachine.CurrentState == PlayingState)
            {
                character.StopSteering();
                spotlightInput.Reset(Mouse.current);
            }
        }

        private void OnGUI()
        {
            if (form == null)
                return;

            GUILayout.BeginArea(new Rect(12f, 12f, Mathf.Min(580f, Screen.width - 24f), 215f), GUI.skin.box);
            GUILayout.Label("A / D  Move     W  Jump     Left Shift  Change form");
            GUILayout.Label("Hold left mouse  Drag lamp     Hold right mouse  Rotate beam");
            GUILayout.Label("R  Restart     Esc  Pause / resume");
            GUILayout.Label($"Level: {StateName}     Form: {form.StateName}     Movement: {character.StateName}");
            if (StateMachine.CurrentState == PausedState)
                GUILayout.Label("Paused. Press Esc to continue.");
            if (form.IsOutsideLight)
                GUILayout.Label($"Outside light: {form.TimeRemaining:0.00}s remaining");
            if (character.IsMovementBlocked)
                GUILayout.Label("Movement blocked by the current form's collision.");
            GUILayout.Label(obstacleHint);
            if (StateMachine.CurrentState == PlayingState && fanSwitch != null && updraft != null
                && (character.FootPosition - (Vector2)fanSwitch.transform.position).sqrMagnitude < 16f)
                GUILayout.Label(updraft.IsActive
                    ? "Fan powered. Aim the beam along the updraft, then enter as Shadow."
                    : "Step on the green switch as Physical to power the fan.");
            GUILayout.Label(feedback);
            GUILayout.EndArea();
        }
    }
}
