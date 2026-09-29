using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PivotAscent
{
    public sealed class PivotLevel : MonoBehaviour
    {
        [Header("Level identity")] public int levelNumber; public string levelTitle = "Untitled Ascent"; public int targetSeconds; public int gemTotal; public bool isDailyChallenge;
        [Header("Vertical play bounds")] public float minimumY = -.67f; public float maximumY = 7f;
        [HideInInspector] public int gemsCollected;
        Text levelText; Text gemText; Text timeText; Text hintText; Text alertText; GameObject hudRoot; Canvas canvas; RectTransform replayButton; RectTransform nextButton; RectTransform homeButton; PivotPlayer player; Vector2 previousPivot; Vector2 previousSwing; bool hasPreviousPositions; float startedAt; bool completed; bool resolving;
        public bool IsComplete => completed;
        // Dynamic levels set their values in Awake.  Delaying the HUD one frame
        // ensures those values are ready before any UI is drawn.
        void Awake() { Application.targetFrameRate = 60; }
        void Start()
        {
            if (string.IsNullOrWhiteSpace(levelTitle)) levelTitle = isDailyChallenge ? "Daily Challenge" : "Untitled Ascent";
            CreateHud();
            startedAt = Time.time;
        }
        void Update()
        {
            if (!completed && !resolving) CheckHazardContact();
            if (!completed)
            {
                levelText.text = isDailyChallenge ? "DAILY" : $"LEVEL {levelNumber:00}";
                gemText.text = isDailyChallenge ? $"SCORE  {gemsCollected}/{gemTotal}" : $"GEMS  {gemsCollected}/{gemTotal}";
                timeText.text = $"{(Time.time - startedAt):0.0}s";
            }
            if (completed && PivotInput.TapDown) HandleCompletionTap();
        }

        // This is a level-wide safety net. It checks both pivot nodes even when a
        // platform has been moved directly by a script, which avoids relying on
        // Unity's trigger callback timing for a reset.
        void CheckHazardContact()
        {
            if (player == null) player = FindFirstObjectByType<PivotPlayer>();
            if (player == null || player.pivot == null || player.swingNode == null) return;

            Vector2 pivotPosition = player.pivot.position;
            Vector2 swingPosition = player.swingNode.position;
            if (!hasPreviousPositions)
            {
                previousPivot = pivotPosition;
                previousSwing = swingPosition;
                hasPreviousPositions = true;
                return;
            }

            foreach (var hazard in FindObjectsByType<HazardObstacle>(FindObjectsSortMode.None))
            {
                if (hazard.TouchesPath(previousPivot, pivotPosition, .26f) || hazard.TouchesPath(previousSwing, swingPosition, .26f))
                {
                    Fail();
                    break;
                }
            }

            previousPivot = pivotPosition;
            previousSwing = swingPosition;
        }
        public void CollectGem() { gemsCollected++; GameAudio.PlayGem(); }
        public void Complete() { if (completed) return; completed = true; resolving = true; if (isDailyChallenge) DailyChallengeState.MarkCompleted(gemsCollected, Time.time - startedAt); else LevelCatalog.UnlockNext(levelNumber); hudRoot.SetActive(false); CreateCompletionPanel(); }
        public void Fail()
        {
            if (resolving) return;
            resolving = true;
            GameAudio.PlayObstacleHit();
            hintText.gameObject.SetActive(false);
            alertText.gameObject.SetActive(true);
            alertText.text = "OBSTACLE HIT  •  RESETTING";
            Camera.main?.GetComponent<HighWaterCamera>()?.Shake();
            StartCoroutine(ReloadAfterDelay());
        }
        IEnumerator ReloadAfterDelay() { yield return new WaitForSeconds(.5f); SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
        void Replay() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        void NextLevel() { if (levelNumber >= LevelCatalog.TotalLevels) LevelCatalog.OpenMainMenu(); else LevelCatalog.LoadLevel(levelNumber + 1); }
        void HandleCompletionTap() { Vector2 p = PivotInput.PointerPosition; if (replayButton != null && RectTransformUtility.RectangleContainsScreenPoint(replayButton, p)) Replay(); else if (nextButton != null && RectTransformUtility.RectangleContainsScreenPoint(nextButton, p)) NextLevel(); else if (homeButton != null && RectTransformUtility.RectangleContainsScreenPoint(homeButton, p)) LevelCatalog.OpenMainMenu(); }
        void CreateHud()
        {
            var canvasObject = new GameObject("Gameplay HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            hudRoot = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaLayout));
            hudRoot.transform.SetParent(canvas.transform, false);

            var levelCard = CreatePanel(hudRoot.transform, "Level Card", new Vector2(.04f, .90f), new Vector2(.56f, .98f), new Color(.025f, .07f, .14f, .91f));
            levelText = CreateHudText(levelCard, "Level", 38, TextAnchor.UpperLeft, new Color(.88f, .96f, 1f), new Vector2(28, -18), new Vector2(-28, -8));
            var name = CreateHudText(levelCard, "Name", 23, TextAnchor.LowerLeft, new Color(.32f, .86f, 1f), new Vector2(28, 8), new Vector2(-28, 16));
            levelText.rectTransform.anchorMin = new Vector2(0f, .42f); levelText.rectTransform.anchorMax = Vector2.one;
            levelText.rectTransform.offsetMin = new Vector2(28f, -4f); levelText.rectTransform.offsetMax = new Vector2(-28f, 0f);
            name.rectTransform.anchorMin = Vector2.zero; name.rectTransform.anchorMax = new Vector2(1f, .52f);
            name.rectTransform.offsetMin = new Vector2(28f, 6f); name.rectTransform.offsetMax = new Vector2(-28f, 0f);
            name.text = levelTitle.ToUpperInvariant();

            var gemCard = CreatePanel(hudRoot.transform, "Gem Card", new Vector2(.61f, .90f), new Vector2(.96f, .98f), new Color(.025f, .07f, .14f, .91f));
            gemText = CreateHudText(gemCard, "Gems", 28, TextAnchor.MiddleCenter, new Color(.28f, 1f, .66f), new Vector2(18, 0), new Vector2(-18, 0));

            var timeCard = CreatePanel(hudRoot.transform, "Time Card", new Vector2(.04f, .842f), new Vector2(.23f, .883f), new Color(.025f, .07f, .14f, .78f));
            timeText = CreateHudText(timeCard, "Time", 23, TextAnchor.MiddleCenter, new Color(.76f, .85f, 1f), Vector2.zero, Vector2.zero);

            var hintCard = CreatePanel(hudRoot.transform, "Control Hint", new Vector2(.18f, .035f), new Vector2(.82f, .085f), new Color(.025f, .07f, .14f, .82f));
            hintText = CreateHudText(hintCard, "Hint", 23, TextAnchor.MiddleCenter, new Color(.72f, .82f, .94f), Vector2.zero, Vector2.zero);
            hintText.text = "TAP ANYWHERE TO PIVOT";

            alertText = CreateHudText(hudRoot.transform, "Reset Alert", 29, TextAnchor.MiddleCenter, new Color(1f, .36f, .4f), new Vector2(.08f, .47f), new Vector2(.92f, .53f));
            var alertRect = alertText.rectTransform;
            alertRect.anchorMin = new Vector2(.08f, .47f);
            alertRect.anchorMax = new Vector2(.92f, .53f);
            alertRect.offsetMin = alertRect.offsetMax = Vector2.zero;
            alertText.gameObject.SetActive(false);
        }

        RectTransform CreatePanel(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            var panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = panel.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            panel.AddComponent<UiEntranceAnimation>();
            return rect;
        }

        Text CreateHudText(Transform parent, string name, int size, TextAnchor alignment, Color color, Vector2 minimum, Vector2 maximum)
        {
            var text = new GameObject(name, typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size; text.alignment = alignment; text.color = color;
            var rect = text.rectTransform; rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = minimum; rect.offsetMax = maximum;
            return text;
        }
        void CreateCompletionPanel() { bool finalLevel = !isDailyChallenge && levelNumber >= LevelCatalog.TotalLevels; string clearTitle = isDailyChallenge ? "DAILY CLEAR" : finalLevel ? "GAME COMPLETE" : "LEVEL CLEAR"; var panel = new GameObject("Completion Panel", typeof(Image)); panel.transform.SetParent(canvas.transform, false); var pr = panel.GetComponent<RectTransform>(); pr.anchorMin = new Vector2(.08f, .28f); pr.anchorMax = new Vector2(.92f, .72f); pr.offsetMin = pr.offsetMax = Vector2.zero; panel.GetComponent<Image>().color = new Color(.035f, .075f, .14f, .96f); panel.AddComponent<UiEntranceAnimation>(); var title = CreateText(clearTitle, 62, TextAnchor.UpperCenter); title.transform.SetParent(panel.transform, false); var tr = title.rectTransform; tr.anchorMin = new Vector2(0, .65f); tr.anchorMax = new Vector2(1, .94f); tr.offsetMin = tr.offsetMax = Vector2.zero; string summaryText = finalLevel ? $"Summit reached\nGems collected  {gemsCollected}/{gemTotal}\n\nThank you for playing." : isDailyChallenge ? $"Daily summit reached\nScore  {gemsCollected}/{gemTotal}  •  Time  {Time.time - startedAt:0.0}s\n\nCome back tomorrow for a new climb." : $"Summit reached\nGems collected  {gemsCollected}/{gemTotal}"; var summary = CreateText(summaryText, 32, TextAnchor.MiddleCenter); summary.transform.SetParent(panel.transform, false); var sr = summary.rectTransform; sr.anchorMin = new Vector2(0, .36f); sr.anchorMax = new Vector2(1, .63f); sr.offsetMin = sr.offsetMax = Vector2.zero; if (finalLevel) { replayButton = CreateButton(panel.transform, "Play Level 1", new Vector2(.35f, .13f), () => LevelCatalog.LoadLevel(1), new Color(.12f, .62f, .42f)); homeButton = CreateButton(panel.transform, "Home", new Vector2(.67f, .13f), LevelCatalog.OpenMainMenu, new Color(.08f, .22f, .38f)); } else if (isDailyChallenge) { replayButton = CreateButton(panel.transform, "Replay Daily", new Vector2(.35f, .13f), Replay, new Color(.16f, .35f, .55f)); homeButton = CreateButton(panel.transform, "Home", new Vector2(.67f, .13f), LevelCatalog.OpenMainMenu, new Color(.08f, .22f, .38f)); } else { replayButton = CreateButton(panel.transform, "Replay", new Vector2(.18f, .13f), Replay, new Color(.16f, .35f, .55f)); homeButton = CreateButton(panel.transform, "Home", new Vector2(.5f, .13f), LevelCatalog.OpenMainMenu, new Color(.08f, .22f, .38f)); nextButton = CreateButton(panel.transform, "Next", new Vector2(.82f, .13f), NextLevel, new Color(.12f, .62f, .42f)); } }
        Text CreateText(string value, int size, TextAnchor align) { var text = new GameObject("Text", typeof(Text)).GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.text = value; text.fontSize = size; text.alignment = align; text.color = Color.white; return text; }
        RectTransform CreateButton(Transform parent, string label, Vector2 anchor, UnityAction action, Color color) { var button = new GameObject(label, typeof(Image), typeof(Button)); button.transform.SetParent(parent, false); var rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.sizeDelta = new Vector2(260, 100); button.GetComponent<Image>().color = color; button.GetComponent<Button>().onClick.AddListener(action); button.AddComponent<UiEntranceAnimation>(); var text = CreateText(label, 28, TextAnchor.MiddleCenter); text.transform.SetParent(button.transform, false); text.rectTransform.anchorMin = Vector2.zero; text.rectTransform.anchorMax = Vector2.one; text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero; return rect; }
    }
}
