using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowShift
{
    public sealed class StudioDemoMenu : MonoBehaviour
    {
        private const string MainMenuScene = "Assets/Scenes/MainMenu.unity";

        [SerializeField] private LevelSession level;
        [SerializeField] private CanvasGroup panel;
        [SerializeField] private RectTransform gameplayHintPanel;
        [SerializeField] private Text title;
        [SerializeField] private Text subtitle;
        [SerializeField] private MainMenuButton resumeButton;
        [SerializeField] private MainMenuButton nextLevelButton;
        [SerializeField] private MainMenuButton restartButton;
        [SerializeField] private MainMenuButton mainMenuButton;
        [SerializeField] private Text restartLabel;
        [SerializeField] private string levelLabel = "Studio Demo";
        [SerializeField, Tooltip("Optional scene path loaded by the Next Level button after completion.")]
        private string nextLevelScene;

        private LevelState displayedState;
        private bool loadingMenu;

        private void Awake()
        {
            SetPanelVisible(false);
        }

        private void LateUpdate()
        {
            if (loadingMenu)
                return;

            LevelState state = level.StateMachine.CurrentState;
            if (state == displayedState)
                return;

            displayedState = state;
            bool paused = state == level.PausedState;
            bool completed = state == level.CompletedState;
            if (!paused && !completed)
            {
                SetPanelVisible(false);
                return;
            }

            bool hasNextLevel = completed && nextLevelButton != null && !string.IsNullOrEmpty(nextLevelScene);
            title.text = completed ? (hasNextLevel ? "Stage clear" : "All stages clear") : "Paused";
            restartLabel.text = completed ? "Play again" : "Restart";
            resumeButton.gameObject.SetActive(paused);
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(hasNextLevel);
            restartButton.interactable = true;
            string shadowStatus = FormatShadowTime();
            subtitle.text = completed ? levelLabel : (shadowStatus.Length > 0 ? shadowStatus : levelLabel);
            subtitle.gameObject.SetActive(subtitle.text.Length > 0);
            SetPanelVisible(true);

            var firstButton = paused ? resumeButton : (hasNextLevel ? nextLevelButton : mainMenuButton);
            EventSystem.current?.SetSelectedGameObject(firstButton.gameObject);
        }

        public void ResumeLevel()
        {
            if (!loadingMenu && level.PausedState != null && level.StateMachine.CurrentState == level.PausedState)
                level.StateMachine.ChangeState(level.PlayingState);
        }

        public void OpenNextLevel()
        {
            if (loadingMenu || level.StateMachine.CurrentState != level.CompletedState || string.IsNullOrEmpty(nextLevelScene))
                return;

            if (!Application.CanStreamedLevelBeLoaded(nextLevelScene))
            {
                Debug.LogError($"Add {nextLevelScene} to the build scene list before advancing.", this);
                return;
            }

            loadingMenu = true;
            panel.interactable = false;
            level.enabled = false;
            SceneManager.LoadSceneAsync(nextLevelScene);
        }

        public void RestartLevel()
        {
            if (!loadingMenu && IsPauseOrCompletion())
                level.RestartLevel();
        }

        public void ReturnToMainMenu()
        {
            if (loadingMenu || !IsPauseOrCompletion())
                return;

            if (!Application.CanStreamedLevelBeLoaded(MainMenuScene))
            {
                Debug.LogError("Add MainMenu to the build scene list before returning from Studio Demo.", this);
                return;
            }

            loadingMenu = true;
            panel.interactable = false;
            level.enabled = false;
            SceneManager.LoadSceneAsync(MainMenuScene);
        }

        private bool IsPauseOrCompletion()
        {
            LevelState state = level.StateMachine.CurrentState;
            return state != null && (state == level.PausedState || state == level.CompletedState);
        }

        private string FormatShadowTime()
        {
            return level.ShadowTimeRemaining is float seconds ? $"Shadow stability · {seconds:0.0}s" : string.Empty;
        }

        private void SetPanelVisible(bool visible)
        {
            var eventSystem = EventSystem.current;
            var selected = eventSystem?.currentSelectedGameObject;
            if (!visible && selected != null && selected.transform.IsChildOf(panel.transform))
                eventSystem.SetSelectedGameObject(null);

            panel.alpha = visible ? 1f : 0f;
            panel.interactable = visible;
            panel.blocksRaycasts = visible;
            if (gameplayHintPanel != null)
                gameplayHintPanel.gameObject.SetActive(!visible);
        }
    }
}
