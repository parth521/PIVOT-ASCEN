using UnityEditor;
using UnityEngine;
using PivotAscent;

public static class ProgressResetMenu
{
    [MenuItem("PIVOT ASCENT/Reset Test Progress (Level 1 Only)", false, 200)]
    static void ResetTestProgress()
    {
        LevelCatalog.ResetProgress();
        Debug.Log("PIVOT ASCENT test progress reset: only Level 1 is unlocked.");
    }
}
