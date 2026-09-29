using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using PivotAscent;

public static class PivotLevelSceneGenerator
{
    static readonly string[] Titles = { "First Steps", "Alternating Anchors", "The Narrow Chimney", "The First Gate", "Zig-Zag Ascent", "The Rotating Blade", "Horizontal Sweeper", "Floating Anchor Islands", "Double Gate Pass", "Pulsing Laser Barriers", "The Spiral Tower", "Fast Track Sweepers", "Moving Anchor Ledges", "The Funnel", "Crossfire Chamber", "The Pendulum Corridor", "Shrinking Safe Zones", "Dual Rotary Gauntlet", "The Speed Climb", "The Grand Ascent" };
    static readonly int[] Heights = { 8,15,18,20,22,25,25,28,30,32,35,35,38,40,42,45,48,50,52,60 };
    static readonly int[] Times = { 4,6,7,8,9,10,10,11,12,13,14,12,15,14,16,17,18,19,15,25 };
    static readonly int[] Gems = { 3,4,3,2,5,4,3,4,4,5,6,4,5,4,6,5,5,6,8,10 };
    const string Folder = "Assets/Scenes/Levels";

    [MenuItem("PIVOT ASCENT/Generate Finalized Level 1")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory(Folder);
        GenerateLevel(0);
        var buildScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity") != null)
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true));
        buildScenes.Add(new EditorBuildSettingsScene($"{Folder}/Level_01.unity", true));
        for (int i = 2; i <= LevelCatalog.TotalLevels; i++)
        {
            string path = $"Assets/Level_{i:00}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) buildScenes.Add(new EditorBuildSettingsScene(path, true));
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/DailyChallenge.unity") != null)
            buildScenes.Add(new EditorBuildSettingsScene("Assets/Scenes/DailyChallenge.unity", true));
        EditorBuildSettings.scenes = buildScenes.ToArray();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        Debug.Log("PIVOT ASCENT: generated finalized Level 1 scene.");
    }

    static EditorBuildSettingsScene[] BuildSettingsScenes()
    {
        var scenes = new EditorBuildSettingsScene[20];
        for (int i = 0; i < 20; i++) scenes[i] = new EditorBuildSettingsScene($"{Folder}/Level_{i + 1:00}.unity", true);
        return scenes;
    }

    static void GenerateLevel(int index)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int number = index + 1; float height = Heights[index];
        var levelGo = new GameObject("Level Controller");
        var level = levelGo.AddComponent<PivotLevel>(); level.levelNumber = number; level.levelTitle = Titles[index]; level.targetSeconds = Times[index]; level.gemTotal = Gems[index]; level.minimumY = -.67f; level.maximumY = height - .3f;
        Camera cam = new GameObject("Portrait Camera").AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 8.9f; cam.transform.position = new Vector3(0, 6, -10); cam.backgroundColor = new Color(.025f,.04f,.09f); cam.clearFlags = CameraClearFlags.SolidColor; cam.tag = "MainCamera";
        var follow = cam.gameObject.AddComponent<HighWaterCamera>();
        follow.player = CreatePlayer(levelGo.transform, level);
        CreateWalls(height, index);
        CreateHorizontalBoundaries(height);
        CreateLayout(index, height, level);
        EditorSceneManager.SaveScene(scene, $"{Folder}/Level_{number:00}.unity");
    }

    static PivotPlayer CreatePlayer(Transform parent, PivotLevel level)
    {
        var root = new GameObject("Pivot Player"); root.transform.SetParent(parent);
        var pivot = Circle("Pivot Node", new Vector3(0, 1), .22f, new Color(.2f,.95f,1));
        var swing = Circle("Swing Node", new Vector3(1.65f, -.67f), .22f, new Color(1f,.86f,.25f));
        pivot.AddComponent<CircleCollider2D>().isTrigger = true; swing.AddComponent<CircleCollider2D>().isTrigger = true;
        var pivotBody = pivot.AddComponent<Rigidbody2D>(); pivotBody.bodyType = RigidbodyType2D.Kinematic; pivotBody.useFullKinematicContacts = true;
        var swingBody = swing.AddComponent<Rigidbody2D>(); swingBody.bodyType = RigidbodyType2D.Kinematic; swingBody.useFullKinematicContacts = true;
        pivot.AddComponent<PivotNode>().level = level; swing.AddComponent<PivotNode>().level = level;
        var line = root.AddComponent<LineRenderer>(); line.positionCount = 2; line.startWidth = line.endWidth = .075f; line.material = new Material(Shader.Find("Sprites/Default")); line.startColor = line.endColor = new Color(.75f,.9f,1);
        var player = root.AddComponent<PivotPlayer>(); player.pivot = pivot.transform; player.swingNode = swing.transform; player.rod = line; player.level = level;
        return player;
    }

    static void CreateWalls(float height, int index)
    {
        float half = index == 2 ? 3.0f : (index == 13 ? 3.7f : 4.75f);
        var left = Block("Left Boundary", new Vector3(-half, height / 2f), new Vector2(.35f, height), new Color(.12f,.22f,.35f)); left.AddComponent<ViewportSideWall>().side = -1;
        var right = Block("Right Boundary", new Vector3(half, height / 2f), new Vector2(.35f, height), new Color(.12f,.22f,.35f)); right.AddComponent<ViewportSideWall>().side = 1;
    }

    static void CreateHorizontalBoundaries(float height)
    {
        var bottom = CreateHorizontalBoundary("Bottom Boundary", -1f);
        var top = CreateHorizontalBoundary("Top Boundary", height - .1f);
    }

    static GameObject CreateHorizontalBoundary(string name, float y)
    {
        var boundary = new GameObject(name); boundary.transform.position = new Vector3(0, y, 0); boundary.transform.localScale = new Vector3(10f, .22f, 1f);
        boundary.AddComponent<BoxCollider2D>().size = Vector2.one; boundary.AddComponent<ViewportHorizontalBoundary>();
        return boundary;
    }

    static void CreateLayout(int index, float height, PivotLevel level)
    {
        if (index == 0)
        {
            Gem(new Vector3(0f, 2.4f));
            Gem(new Vector3(0f, 3.6f));
            Gem(new Vector3(0f, 4.8f));
            GoalDot(new Vector3(0f, 6.3f), level);
            return;
        }
        int gemCount = Gems[index];
        int hazardCount = Mathf.Max(0, index - 1); // Starts clear, then adds one challenge at a time.
        for (int i = 0; i < gemCount; i++)
        {
            float y = 2.3f + (height - 4.2f) * (i + 1) / (gemCount + 1);
            float x = index < 2 ? 0f : (i % 2 == 0 ? -.8f : .8f);
            Gem(new Vector3(x, y));
        }

        for (int i = 0; i < hazardCount; i++)
        {
            float y = 3.3f + (height - 6.2f) * (i + 1) / (hazardCount + 1);
            float x = i % 2 == 0 ? -2.5f : 2.5f;
            Vector3 travel = Vector3.zero;
            // Static hazards appear first; later levels add horizontal, vertical and diagonal motion.
            if (index >= 4)
            {
                int pattern = (i + index) % 4;
                travel = pattern == 1 ? Vector3.right * 2.2f : pattern == 2 ? Vector3.up * 1.6f : pattern == 3 ? new Vector3(1.7f, 1.2f) : Vector3.zero;
            }
            CreateHazard($"Hazard {i + 1}", new Vector3(x, y), new Vector2(1.15f, .32f), travel, .35f + index * .035f);
        }

        // Gates begin midway through the campaign and force controlled timing near the summit.
        if (index >= 7)
        {
            float gateY = height * .7f;
            CreateHazard("Gate Left", new Vector3(-2.85f, gateY), new Vector2(2.3f, .3f), index >= 11 ? Vector3.right * 1.2f : Vector3.zero, .6f);
            CreateHazard("Gate Right", new Vector3(2.85f, gateY), new Vector2(2.3f, .3f), index >= 13 ? Vector3.left * 1.2f : Vector3.zero, .6f);
        }

        GoalDot(new Vector3(0, height - 1.2f), level);
    }

    static GameObject Circle(string name, Vector3 pos, float scale, Color color)
    { var go = new GameObject(name); go.transform.position = pos; var sr = go.AddComponent<SpriteRenderer>(); sr.color = color; go.AddComponent<PivotVisual>(); go.transform.localScale = Vector3.one * scale * 2; return go; }
    static GameObject Block(string name, Vector3 pos, Vector2 size, Color color)
    { var go = new GameObject(name); go.transform.position = pos; var sr = go.AddComponent<SpriteRenderer>(); sr.color = color; go.AddComponent<PivotVisual>(); go.transform.localScale = size; return go; }
    static void Gem(Vector3 pos) { var go = Circle("Gem", pos, .13f, new Color(.2f,1f,.68f)); go.AddComponent<GemPickup>(); go.AddComponent<CircleCollider2D>().isTrigger = true; }
    static void Goal(Vector3 pos) { var go = Circle("Summit Goal", pos, .38f, new Color(.15f,1f,.42f)); go.AddComponent<SummitGoal>(); go.AddComponent<CircleCollider2D>().isTrigger = true; }
    static void CreateHazard(string name, Vector3 pos, Vector2 size, Vector3 travel, float frequency)
    {
        var hazard = Block(name, pos, size, new Color(.95f, .15f, .25f));
        hazard.AddComponent<HazardObstacle>(); hazard.AddComponent<BoxCollider2D>().isTrigger = true;
        if (travel != Vector3.zero) { var movement = hazard.AddComponent<Oscillator>(); movement.travel = travel; movement.frequency = frequency; }
    }

    static void GoalDot(Vector3 pos, PivotLevel level)
    {
        var go = new GameObject("Summit Goal"); go.transform.position = pos; go.transform.localScale = Vector3.one * 1.25f;
        go.AddComponent<CircleCollider2D>().isTrigger = true; go.AddComponent<SummitGoal>().level = level; go.AddComponent<SummitDotVisual>();
        var label = new GameObject("Summit Label"); label.transform.position = pos + Vector3.up * .92f;
        var text = label.AddComponent<TextMesh>(); text.text = "SUMMIT"; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 46; text.characterSize = .12f; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = new Color(.25f, 1f, .58f);
    }
}
