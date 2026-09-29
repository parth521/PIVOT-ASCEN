using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DailyChallengeSceneGenerator
{
    const string DailyPath = "Assets/Scenes/DailyChallenge.unity";

    [MenuItem("PIVOT ASCENT/Create Daily Challenge Scene")]
    public static void Create()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Daily Challenge Generator").AddComponent<PivotAscent.DailyChallengeGenerator>();
        EditorSceneManager.SaveScene(scene, DailyPath);
        AssetDatabase.Refresh();
    }
}
