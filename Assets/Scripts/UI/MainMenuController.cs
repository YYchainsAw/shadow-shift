using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ShadowShift
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private const string StudioDemoScene = "Assets/Scenes/StudioDemo.unity";
        private static readonly Color ShadowTint = new Color32(183, 161, 194, 255);

        [SerializeField] private CanvasGroup home;
        [SerializeField] private CanvasGroup levelSelect;
        [SerializeField] private MainMenuButton playButton;
        [SerializeField] private MainMenuButton firstLevelButton;
        [SerializeField] private MenuSpotlight spotlight;
        [SerializeField] private Image protagonist;
        [SerializeField, Min(0.05f)] private float transitionDuration = 0.7f;

        private CanvasGroup activePage;
        private CanvasGroup destination;
        private MainMenuButton nextFocus;
        private float transitionProgress;
        private bool loadingLevel;
        private Color protagonistColor;

        private void Awake()
        {
            activePage = home;
            protagonistColor = protagonist.color;
            home.alpha = 1f;
            SetInputEnabled(home, true);
            HidePage(levelSelect);
            UpdateSpotlight(0f);
        }

        public void ShowLevels() => OpenPage(levelSelect, firstLevelButton);

        public void ShowMainMenu()
        {
            if (activePage != home && destination == null && !loadingLevel)
                BeginTransition(home, playButton);
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
            transitionProgress = Mathf.MoveTowards(transitionProgress, 1f, Time.unscaledDeltaTime / transitionDuration);
            float progress = returningHome ? 1f - transitionProgress : transitionProgress;

            var detailPage = returningHome ? activePage : destination;
            home.alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, 0.32f, progress));
            detailPage.alpha = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.62f, 0.96f, progress));
            UpdateSpotlight(progress);

            if (transitionProgress < 1f)
                return;

            activePage = destination;
            destination = null;
            SetInputEnabled(activePage, true);
            EventSystem.current?.SetSelectedGameObject(nextFocus.gameObject);
        }

        private void OpenPage(CanvasGroup page, MainMenuButton focus)
        {
            if (activePage != home || destination != null)
                return;

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
