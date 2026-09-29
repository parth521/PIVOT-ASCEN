using System;
using UnityEngine;

namespace PivotAscent
{
    public static class DailyChallengeState
    {
        const string CompletedDayKey = "PivotDailyCompleted";
        const string ResultDayKey = "PivotDailyResultDay";
        const string ScoreKey = "PivotDailyScore";
        const string TimeKey = "PivotDailyTime";

        public static string DayId => DateTime.UtcNow.ToString("yyyyMMdd");
        public static int Seed => int.Parse(DayId);
        public static bool IsCompleted => PlayerPrefs.GetString(CompletedDayKey, string.Empty) == DayId;
        public static bool HasResultToday => PlayerPrefs.GetString(ResultDayKey, string.Empty) == DayId;
        public static int ScoreToday => HasResultToday ? PlayerPrefs.GetInt(ScoreKey, 0) : 0;
        public static float TimeToday => HasResultToday ? PlayerPrefs.GetFloat(TimeKey, 0f) : 0f;

        public static void MarkCompleted(int score, float seconds)
        {
            PlayerPrefs.SetString(CompletedDayKey, DayId);
            bool shouldReplace = !HasResultToday || score > ScoreToday || (score == ScoreToday && (TimeToday <= 0f || seconds < TimeToday));
            if (shouldReplace)
            {
                PlayerPrefs.SetString(ResultDayKey, DayId);
                PlayerPrefs.SetInt(ScoreKey, score);
                PlayerPrefs.SetFloat(TimeKey, seconds);
            }
            PlayerPrefs.Save();
        }
    }
}
