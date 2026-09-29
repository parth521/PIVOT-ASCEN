using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PivotAscent
{
    public sealed class MainMenuController : MonoBehaviour
    {
        readonly List<TapTarget> tapTargets = new List<TapTarget>();
        GameObject homeScreen;
        GameObject levelScreen;
        GameObject leaderboardScreen;
        GameObject settingsScreen;
        Font font;

        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            CreateUi();
            ShowHome();
        }

        void Update()
        {
            if (!PivotInput.TapDown) return;
            Vector2 point = PivotInput.PointerPosition;
            for (int i = tapTargets.Count - 1; i >= 0; i--)
            {
                var target = tapTargets[i];
                if (target.rect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(target.rect, point))
                {
                    target.action();
                    return;
                }
            }
        }

        void CreateUi()
        {
            var canvasObject = new GameObject("Main Menu UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);

            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaLayout));
            safe.transform.SetParent(canvas.transform, false);
            homeScreen = CreateScreen("Home Screen", safe.transform);
            levelScreen = CreateScreen("Level Select Screen", safe.transform);
            leaderboardScreen = CreateScreen("Leaderboard Screen", safe.transform);
            settingsScreen = CreateScreen("Settings Screen", safe.transform);
            CreateHome();
            CreateLevelSelect();
            CreateLeaderboard();
            CreateSettings();
        }

        void CreateHome()
        {
            // A dedicated header card keeps the game name legible below every
            // iPhone notch and Dynamic Island rather than relying on bare text.
            var titleHeader = CreatePanel(homeScreen.transform, "Game Title Header", new Vector2(.08f, .79f), new Vector2(.92f, .88f), new Color(.025f, .07f, .14f, .92f));
            CreateText(titleHeader, "Game Name", 58, TextAnchor.MiddleCenter, new Color(.25f, .93f, 1f), Vector2.zero, Vector2.one).text = "PIVOT ASCENT";
            CreateText(homeScreen.transform, "A neon climb to the summit", 22, TextAnchor.MiddleCenter, new Color(.67f, .77f, .93f), new Vector2(.08f, .745f), new Vector2(.92f, .775f));

            AddButton(homeScreen.transform, "Play", "PLAY", "CHOOSE A LEVEL", new Vector2(.12f, .49f), new Vector2(.88f, .57f), new Color(.10f, .65f, .48f), ShowLevelSelect);
            AddButton(homeScreen.transform, "Daily", "DAILY CHALLENGE", DailyChallengeState.IsCompleted ? "TODAY'S SUMMIT CLEARED" : "A NEW ASCENT EVERY DAY", new Vector2(.12f, .39f), new Vector2(.88f, .47f), new Color(.20f, .20f, .56f), () => UnityEngine.SceneManagement.SceneManager.LoadScene("DailyChallenge"));
            AddButton(homeScreen.transform, "Leaderboard", "LEADERBOARD", "DAILY RANKINGS", new Vector2(.12f, .29f), new Vector2(.88f, .37f), new Color(.08f, .22f, .38f), ShowLeaderboard);
            AddButton(homeScreen.transform, "Settings", "SETTINGS", "SOUND OPTIONS", new Vector2(.12f, .19f), new Vector2(.88f, .27f), new Color(.08f, .13f, .24f), ShowSettings);
            var progress = CreatePanel(homeScreen.transform, "Progress", new Vector2(.18f, .12f), new Vector2(.82f, .16f), new Color(.025f, .07f, .14f, .8f));
            CreateText(progress, "Progress", 25, TextAnchor.MiddleCenter, new Color(.75f, .86f, 1f), Vector2.zero, Vector2.one).text = $"{LevelCatalog.UnlockedLevel}/{LevelCatalog.TotalLevels} LEVELS UNLOCKED";
            CreateText(homeScreen.transform, "Footer", 20, TextAnchor.MiddleCenter, new Color(.42f, .55f, .72f), new Vector2(.08f, .06f), new Vector2(.92f, .10f)).text = "TAP TO BEGIN YOUR ASCENT";
        }

        void CreateLevelSelect()
        {
            AddButton(levelScreen.transform, "Back", "‹  BACK", "", new Vector2(.05f, .90f), new Vector2(.28f, .96f), new Color(.08f, .22f, .38f), ShowHome);
            CreateText(levelScreen.transform, "Heading", 52, TextAnchor.MiddleCenter, new Color(.86f, .95f, 1f), new Vector2(.25f, .89f), new Vector2(.95f, .96f)).text = "SELECT LEVEL";
            CreateText(levelScreen.transform, "Subheading", 23, TextAnchor.MiddleCenter, new Color(.36f, .86f, 1f), new Vector2(.12f, .84f), new Vector2(.88f, .88f)).text = "CLEAR A LEVEL TO UNLOCK THE NEXT";

            int unlocked = LevelCatalog.UnlockedLevel;
            for (int number = 1; number <= LevelCatalog.TotalLevels; number++)
            {
                int column = (number - 1) % 4;
                int row = (number - 1) / 4;
                float xMin = .06f + column * .225f;
                float yMax = .80f - row * .137f;
                bool isUnlocked = number <= unlocked;
                AddLevelCard(number, new Vector2(xMin, yMax - .115f), new Vector2(xMin + .20f, yMax), isUnlocked, number == unlocked);
            }
        }

        void AddLevelCard(int number, Vector2 minimum, Vector2 maximum, bool unlocked, bool newest)
        {
            Color color = unlocked ? (newest ? new Color(.08f, .46f, .54f) : new Color(.045f, .18f, .29f)) : new Color(.045f, .06f, .10f);
            var card = CreatePanel(levelScreen.transform, $"Level {number:00}", minimum, maximum, color);
            CreateText(card, "Number", 35, TextAnchor.MiddleCenter, unlocked ? new Color(.32f, 1f, .71f) : new Color(.37f, .43f, .54f), new Vector2(0f, .40f), Vector2.one).text = $"{number:00}";
            CreateText(card, "Label", 18, TextAnchor.MiddleCenter, unlocked ? new Color(.82f, .92f, 1f) : new Color(.37f, .43f, .54f), Vector2.zero, new Vector2(1f, .42f)).text = unlocked ? "PLAY" : "LOCKED";
            if (unlocked) tapTargets.Add(new TapTarget(card, () => LevelCatalog.LoadLevel(number)));
        }

        void CreateLeaderboard()
        {
            AddButton(leaderboardScreen.transform, "Back", "‹  BACK", "", new Vector2(.05f, .90f), new Vector2(.28f, .96f), new Color(.08f, .22f, .38f), ShowHome);
            CreateText(leaderboardScreen.transform, "Heading", 52, TextAnchor.MiddleCenter, new Color(.86f, .95f, 1f), new Vector2(.25f, .89f), new Vector2(.95f, .96f)).text = "DAILY LEADERBOARD";
            CreateText(leaderboardScreen.transform, "Subheading", 23, TextAnchor.MiddleCenter, new Color(.36f, .86f, 1f), new Vector2(.12f, .84f), new Vector2(.88f, .88f)).text = $"{DateTime.UtcNow:dd MMM}  •  DAILY CHALLENGE";

            var entries = new List<LeaderboardEntry>
            {
                new LeaderboardEntry("NOVA", 6, 21.4f),
                new LeaderboardEntry("SKYLINE", 6, 23.1f),
                new LeaderboardEntry("ORBIT", 6, 25.8f),
                new LeaderboardEntry("VOLT", 6, 31.2f),
                new LeaderboardEntry("MIRAGE", 6, 34.0f),
                new LeaderboardEntry("YOU", DailyChallengeState.ScoreToday, DailyChallengeState.TimeToday)
            };
            entries.Sort(LeaderboardEntry.Compare);
            for (int index = 0; index < entries.Count; index++)
            {
                float top = .78f - index * .095f;
                var entry = entries[index];
                bool isPlayer = entry.name == "YOU";
                var row = CreatePanel(leaderboardScreen.transform, $"Rank {index + 1}", new Vector2(.10f, top - .075f), new Vector2(.90f, top), isPlayer ? new Color(.08f, .40f, .45f) : new Color(.025f, .07f, .14f, .86f));
                CreateText(row, "Rank", 28, TextAnchor.MiddleCenter, new Color(.34f, 1f, .68f), new Vector2(.03f, 0f), new Vector2(.17f, 1f)).text = entry.hasResult ? $"#{index + 1}" : "—";
                CreateText(row, "Name", 29, TextAnchor.MiddleLeft, Color.white, new Vector2(.22f, 0f), new Vector2(.52f, 1f)).text = entry.name;
                CreateText(row, "Score", 22, TextAnchor.MiddleCenter, new Color(.36f, 1f, .72f), new Vector2(.53f, 0f), new Vector2(.72f, 1f)).text = entry.hasResult ? $"{entry.score}/6" : "—";
                CreateText(row, "Time", 27, TextAnchor.MiddleRight, new Color(.74f, .87f, 1f), new Vector2(.73f, 0f), new Vector2(.94f, 1f)).text = entry.hasResult ? $"{entry.time:0.0}s" : "—";
            }
            CreateText(leaderboardScreen.transform, "Note", 20, TextAnchor.MiddleCenter, new Color(.42f, .55f, .72f), new Vector2(.08f, .13f), new Vector2(.92f, .18f)).text = "YOUR SCORE AND TIME ARE SAVED ON THIS DEVICE";
        }

        void CreateSettings()
        {
            AddButton(settingsScreen.transform, "Back", "‹  BACK", "", new Vector2(.05f, .90f), new Vector2(.28f, .96f), new Color(.08f, .22f, .38f), ShowHome);
            CreateText(settingsScreen.transform, "Heading", 52, TextAnchor.MiddleCenter, new Color(.86f, .95f, 1f), new Vector2(.25f, .89f), new Vector2(.95f, .96f)).text = "SETTINGS";
            CreateText(settingsScreen.transform, "Subheading", 24, TextAnchor.MiddleCenter, new Color(.36f, .86f, 1f), new Vector2(.12f, .81f), new Vector2(.88f, .85f)).text = "AUDIO";
            AddButton(settingsScreen.transform, "Sound Toggle", IsMuted ? "SOUND OFF" : "SOUND ON", "TAP TO TOGGLE", new Vector2(.12f, .65f), new Vector2(.88f, .74f), IsMuted ? new Color(.30f, .10f, .18f) : new Color(.10f, .65f, .48f), ToggleMute);
            CreateText(settingsScreen.transform, "Note", 21, TextAnchor.MiddleCenter, new Color(.58f, .68f, .83f), new Vector2(.12f, .55f), new Vector2(.88f, .60f)).text = "MUSIC AND SOUND EFFECTS";
        }

        bool IsMuted => PlayerPrefs.GetInt("PivotMuted", 0) == 1;

        void ToggleMute()
        {
            bool muted = !IsMuted;
            PlayerPrefs.SetInt("PivotMuted", muted ? 1 : 0);
            PlayerPrefs.Save();
            AudioListener.pause = muted;
            var toggle = settingsScreen.transform.Find("Sound Toggle");
            toggle.GetComponent<Image>().color = muted ? new Color(.30f, .10f, .18f) : new Color(.10f, .65f, .48f);
            toggle.Find("Title").GetComponent<Text>().text = muted ? "SOUND OFF" : "SOUND ON";
        }

        void ShowHome()
        {
            homeScreen.SetActive(true);
            levelScreen.SetActive(false);
            leaderboardScreen.SetActive(false);
            settingsScreen.SetActive(false);
        }

        void ShowLevelSelect()
        {
            homeScreen.SetActive(false);
            levelScreen.SetActive(true);
            leaderboardScreen.SetActive(false);
            settingsScreen.SetActive(false);
        }

        void ShowLeaderboard() { homeScreen.SetActive(false); levelScreen.SetActive(false); leaderboardScreen.SetActive(true); settingsScreen.SetActive(false); }
        void ShowSettings() { homeScreen.SetActive(false); levelScreen.SetActive(false); leaderboardScreen.SetActive(false); settingsScreen.SetActive(true); }

        GameObject CreateScreen(string name, Transform parent)
        {
            var screen = new GameObject(name, typeof(RectTransform));
            screen.transform.SetParent(parent, false);
            var rect = screen.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            screen.AddComponent<UiEntranceAnimation>();
            return screen;
        }

        RectTransform CreatePanel(Transform parent, string name, Vector2 minimum, Vector2 maximum, Color color)
        {
            var panel = new GameObject(name, typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = minimum; rect.anchorMax = maximum; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = panel.GetComponent<Image>(); image.color = color; image.raycastTarget = false;
            panel.gameObject.AddComponent<UiEntranceAnimation>();
            return rect;
        }

        Text CreateText(Transform parent, string name, int size, TextAnchor alignment, Color color, Vector2 minimum, Vector2 maximum)
        {
            var text = new GameObject(name, typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(parent, false);
            text.font = font; text.fontSize = size; text.alignment = alignment; text.color = color;
            var rect = text.rectTransform; rect.anchorMin = minimum; rect.anchorMax = maximum; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return text;
        }

        void AddButton(Transform parent, string name, string title, string subtitle, Vector2 minimum, Vector2 maximum, Color color, Action action)
        {
            var button = CreatePanel(parent, name, minimum, maximum, color);
            CreateText(button, "Title", 37, TextAnchor.MiddleCenter, Color.white, new Vector2(0f, .34f), Vector2.one).text = title;
            if (!string.IsNullOrEmpty(subtitle)) CreateText(button, "Subtitle", 19, TextAnchor.MiddleCenter, new Color(.74f, .94f, 1f), Vector2.zero, new Vector2(1f, .40f)).text = subtitle;
            tapTargets.Add(new TapTarget(button, action));
        }

        readonly struct TapTarget
        {
            public readonly RectTransform rect;
            public readonly Action action;
            public TapTarget(RectTransform rect, Action action) { this.rect = rect; this.action = action; }
        }

        readonly struct LeaderboardEntry
        {
            public readonly string name;
            public readonly int score;
            public readonly float time;
            public bool hasResult => time > 0f;
            public LeaderboardEntry(string name, int score, float time) { this.name = name; this.score = score; this.time = time; }
            public static int Compare(LeaderboardEntry left, LeaderboardEntry right)
            {
                if (left.hasResult != right.hasResult) return right.hasResult.CompareTo(left.hasResult);
                if (!left.hasResult) return string.CompareOrdinal(left.name, right.name);
                int scoreComparison = right.score.CompareTo(left.score);
                return scoreComparison != 0 ? scoreComparison : left.time.CompareTo(right.time);
            }
        }
    }
}
