using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowShift
{
    public sealed class MainMenuController : MonoBehaviour, ICancelHandler
    {
        private const string StudioDemoScene = "Assets/Scenes/StudioDemo.unity";
        private static readonly Color ShadowTint = new Color32(183, 161, 194, 255);

        [SerializeField] private CanvasGroup home;
        [SerializeField] private CanvasGroup levelSelect;
        [SerializeField] private CanvasGroup controls;
        [SerializeField] private CanvasGroup credits;
        [SerializeField] private CanvasGroup quit;
        [SerializeField] private MainMenuButton playButton;
        [SerializeField] private MainMenuButton firstLevelButton;
        [SerializeField] private MainMenuButton controlsBackButton;
        [SerializeField] private MainMenuButton creditsBackButton;
        [SerializeField] private MainMenuButton cancelQuitButton;
        [SerializeField] private MainMenuButton confirmQuitButton;
        [SerializeField] private Text quitPrompt;
        [SerializeField] private MenuSpotlight spotlight;
        [SerializeField] private Image protagonist;
        [SerializeField, Min(0.05f)] private float transitionDuration = 0.7f;

        private CanvasGroup activePage;
        private CanvasGroup destination;
        private MainMenuButton nextFocus;
        private MainMenuButton homeFocus;
        private float transitionProgress;
        private bool loadingLevel;
        private Color protagonistColor;

        private void Awake()
        {
            activePage = home;
            homeFocus = playButton;
            protagonistColor = protagonist.color;
            home.alpha = 1f;
            SetInputEnabled(home, true);
            HidePage(levelSelect);
            HidePage(controls);
            HidePage(credits);
            HidePage(quit);
            UpdateSpotlight(0f);

#if UNITY_WEBGL && !UNITY_EDITOR
            if (quitPrompt != null && cancelQuitButton != null && confirmQuitButton != null)
            {
                quitPrompt.text = "Close this tab to quit";
                var cancelRect = (RectTransform)cancelQuitButton.transform;
                var confirmRect = (RectTransform)confirmQuitButton.transform;
                cancelRect.anchoredPosition = (cancelRect.anchoredPosition + confirmRect.anchoredPosition) * 0.5f;
                cancelQuitButton.navigation = new Navigation { mode = Navigation.Mode.None };
                confirmQuitButton.gameObject.SetActive(false);
            }
#endif
        }

        public void ShowLevels() => OpenPage(levelSelect, firstLevelButton);
        public void ShowControls() => OpenPage(controls, controlsBackButton);
        public void ShowCredits() => OpenPage(credits, creditsBackButton);
        public void ShowQuit() => OpenPage(quit, cancelQuitButton);

        public void ShowMainMenu()
        {
            if (activePage != home && destination == null && !loadingLevel)
                BeginTransition(home, homeFocus);
        }

        public void OnCancel(BaseEventData eventData)
        {
            if (activePage == home)
                return;

            ShowMainMenu();
            eventData.Use();
        }

        public void QuitGame()
        {
            if (activePage != quit || destination != null)
                return;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif !UNITY_WEBGL
            Application.Quit();
#endif
        }

        public void OpenStudioDemo()
        {
            if (activePage != levelSelect || destination != null || loadingLevel)
                return;

            if (!Application.CanStreamedLevelBeLoaded(StudioDemoScene))
            {
                Debug.LogError("Add StudioDemo to the build scene list before starting level 1.", this);
                return;
            }

            loadingLevel = true;
            SetInputEnabled(levelSelect, false);
            SceneManager.LoadSceneAsync(StudioDemoScene);
        }

        private void Update()
        {
            if (destination == null)
                return;

            bool returningHome = destination == home;
            bool changingQuit = activePage == quit || destination == quit;
            float duration = changingQuit ? 0.2f : transitionDuration;
            transitionProgress = Mathf.MoveTowards(transitionProgress, 1f, Time.unscaledDeltaTime / duration);
            float progress = returningHome ? 1f - transitionProgress : transitionProgress;

            if (changingQuit)
            {
                // Keep the home menu visible behind the confirmation while its input is disabled.
                quit.alpha = Mathf.SmoothStep(0f, 1f, progress);
            }
            else
            {
                var detailPage = returningHome ? activePage : destination;
                home.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.32f, progress));
                detailPage.alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 0.96f, progress));
                UpdateSpotlight(progress);
            }

            if (transitionProgress < 1f)
                return;

            activePage = destination;
            destination = null;
            SetInputEnabled(activePage, true);
            EventSystem.current?.SetSelectedGameObject(nextFocus.gameObject);
        }

        private void OpenPage(CanvasGroup page, MainMenuButton focus)
        {
            if (activePage != home || destination != null || page == null || focus == null)
                return;

            var currentSelection = EventSystem.current?.currentSelectedGameObject;
            homeFocus = currentSelection != null && currentSelection.transform.IsChildOf(home.transform) &&
                currentSelection.TryGetComponent<MainMenuButton>(out var selected) ? selected : playButton;

            BeginTransition(page, focus);
        }

        private void BeginTransition(CanvasGroup page, MainMenuButton focus)
        {
            destination = page;
            nextFocus = focus;
            transitionProgress = 0f;
            EventSystem.current?.SetSelectedGameObject(null);
            SetInputEnabled(activePage, false);
        }

        private void UpdateSpotlight(float progress)
        {
            float spotlightProgress = Mathf.SmoothStep(0f, 1f, progress);
            spotlight.SetProgress(spotlightProgress);
            protagonist.color = Color.Lerp(protagonistColor, protagonistColor * ShadowTint, spotlightProgress);
        }

        private static void HidePage(CanvasGroup group)
        {
            if (group == null)
                return;

            group.alpha = 0f;
            SetInputEnabled(group, false);
        }

        private static void SetInputEnabled(CanvasGroup group, bool enabled)
        {
            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }
    }
}
