using UnityEngine.SceneManagement;

namespace PivotAscent
{
    public static class LevelCatalog
    {
        public const int TotalLevels = 20;
        public const string MainMenuScene = "MainMenu";
        const string UnlockKey = "PivotUnlocked";

        public static int UnlockedLevel => UnityEngine.Mathf.Clamp(UnityEngine.PlayerPrefs.GetInt(UnlockKey, 1), 1, TotalLevels);

        public static void UnlockNext(int completedLevel)
        {
            int newUnlock = UnityEngine.Mathf.Clamp(completedLevel + 1, 1, TotalLevels);
            if (newUnlock > UnlockedLevel) UnityEngine.PlayerPrefs.SetInt(UnlockKey, newUnlock);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            UnityEngine.PlayerPrefs.SetInt(UnlockKey, 1);
            UnityEngine.PlayerPrefs.Save();
        }

        public static void LoadLevel(int levelNumber) => SceneManager.LoadScene($"Level_{levelNumber:00}");
        public static void OpenMainMenu() => SceneManager.LoadScene(MainMenuScene);
    }
}
