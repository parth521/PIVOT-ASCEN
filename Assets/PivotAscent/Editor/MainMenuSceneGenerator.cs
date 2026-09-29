using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MainMenuSceneGenerator
{
    const string MenuPath = "Assets/Scenes/MainMenu.unity";

    [MenuItem("PIVOT ASCENT/Create Main Menu Scene")]
    public static void CreateMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Menu Camera").AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.018f, .035f, .08f);
        camera.tag = "MainCamera";
        new GameObject("Main Menu Controller").AddComponent<PivotAscent.MainMenuController>();
        EditorSceneManager.SaveScene(scene, MenuPath);
        AssetDatabase.Refresh();

        var scenes = new List<EditorBuildSettingsScene> { new EditorBuildSettingsScene(MenuPath, true) };
        for (int level = 1; level <= PivotAscent.LevelCatalog.TotalLevels; level++)
        {
            string path = level == 1 ? "Assets/Scenes/Levels/Level_01.unity" : $"Assets/Level_{level:00}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/DailyChallenge.unity") != null)
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/DailyChallenge.unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }
}
