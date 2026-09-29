using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using PivotAscent;
using System.Collections.Generic;

public sealed class PivotLevelEditorWindow : EditorWindow
{
    Vector2 placement = new Vector2(0f, 4f);
    float movementDistance = 2f;
    float movementSpeed = .6f;

    [MenuItem("PIVOT ASCENT/Level Editor")]
    static void Open() => GetWindow<PivotLevelEditorWindow>("Pivot Level Editor");

    [MenuItem("PIVOT ASCENT/Build Level 2 In Current Scene")]
    static void BuildLevelTwoFromMenu() => GetWindow<PivotLevelEditorWindow>("Pivot Level Editor").CreateLevelTwo();

    [MenuItem("PIVOT ASCENT/Rebuild Level 2 Scene")]
    static void RebuildLevelTwoScene()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Level_02.unity", OpenSceneMode.Single);
        GetWindow<PivotLevelEditorWindow>("Pivot Level Editor").CreateLevelTwo();
    }

    [MenuItem("PIVOT ASCENT/Create Level 3 Scene")]
    static void BuildLevelThreeFromMenu() => GetWindow<PivotLevelEditorWindow>("Pivot Level Editor").CreateLevelThree();

    [MenuItem("PIVOT ASCENT/Create Levels 4 to 10")]
    static void BuildLevelsFourToTenFromMenu() => GetWindow<PivotLevelEditorWindow>("Pivot Level Editor").CreateLevelsFourToTen();

    [MenuItem("PIVOT ASCENT/Create Levels 11 to 20")]
    static void BuildLevelsElevenToTwentyFromMenu() => GetWindow<PivotLevelEditorWindow>("Pivot Level Editor").CreateLevelsElevenToTwenty();

    [MenuItem("PIVOT ASCENT/Rebuild Level List")]
    static void RebuildLevelList()
    {
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("PIVOT: ASCENT Level Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Set a position, add an item, then use Unity's Move tool (W) in the Scene view to arrange it. Red hazards reset the level when touched.", MessageType.Info);

        if (GUILayout.Button("Build Level 2 In Current Scene", GUILayout.Height(28))) CreateLevelTwo();
        if (GUILayout.Button("Create Level 3 Scene", GUILayout.Height(28))) CreateLevelThree();
        if (GUILayout.Button("Create Levels 4 to 10", GUILayout.Height(28))) CreateLevelsFourToTen();
        if (GUILayout.Button("Create Levels 11 to 20", GUILayout.Height(28))) CreateLevelsElevenToTwenty();

        placement = EditorGUILayout.Vector2Field("Place at (X, Y)", placement);
        movementDistance = EditorGUILayout.Slider("Movement distance", movementDistance, .5f, 5f);
        movementSpeed = EditorGUILayout.Slider("Movement speed", movementSpeed, .2f, 2f);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Place objects", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Gem")) AddGem();
            if (GUILayout.Button("Add Summit")) AddSummit();
        }
        if (GUILayout.Button("Add Static Red Obstacle")) AddHazard(Vector3.zero, "Static Obstacle");
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Move Left / Right")) AddHazard(Vector3.right * movementDistance, "Horizontal Obstacle");
            if (GUILayout.Button("Move Up / Down")) AddHazard(Vector3.up * movementDistance, "Vertical Obstacle");
        }
        if (GUILayout.Button("Move Diagonally")) AddHazard(new Vector3(movementDistance, movementDistance * .65f), "Diagonal Obstacle");
        if (GUILayout.Button("Add Rotating Red Obstacle")) AddRotatingHazard();

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Level structure", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Add Left Wall")) AddWall(-1);
            if (GUILayout.Button("Add Right Wall")) AddWall(1);
        }
        if (GUILayout.Button("Select Level Controller"))
        {
            var level = FindFirstObjectByType<PivotLevel>();
            if (level != null) Selection.activeGameObject = level.gameObject;
            else EditorUtility.DisplayDialog("No Level Controller", "Open a PIVOT level scene before editing.", "OK");
        }

        EditorGUILayout.Space(8);
        if (GUILayout.Button("Save Current Level", GUILayout.Height(32)))
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene);
            ShowNotification(new GUIContent("Level saved"));
        }
    }

    void AddGem()
    {
        var gem = CreateOutlinedObject("Gem", placement, new Vector2(.28f, .28f), new Color(.2f, 1f, .68f));
        gem.AddComponent<GemPickup>();
        gem.AddComponent<CircleCollider2D>().isTrigger = true;
        Select(gem);
    }

    void AddSummit()
    {
        var summit = new GameObject("Summit Goal");
        Undo.RegisterCreatedObjectUndo(summit, "Add Summit Goal");
        summit.transform.position = placement;
        summit.transform.localScale = Vector3.one * 1.25f;
        summit.AddComponent<CircleCollider2D>().isTrigger = true;
        var goal = summit.AddComponent<SummitGoal>();
        goal.level = FindFirstObjectByType<PivotLevel>();
        summit.AddComponent<SummitDotVisual>();
        var label = new GameObject("Summit Label");
        Undo.RegisterCreatedObjectUndo(label, "Add Summit Label");
        label.transform.position = new Vector3(placement.x, placement.y + .92f, 0);
        var text = label.AddComponent<TextMesh>();
        text.text = "SUMMIT"; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 46; text.characterSize = .12f; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = new Color(.25f, 1f, .58f);
        Select(summit);
    }

    void AddHazard(Vector3 travel, string name)
    {
        var obstacle = CreateOutlinedObject(name, placement, new Vector2(1.2f, .34f), new Color(.95f, .15f, .25f));
        obstacle.AddComponent<BoxCollider2D>().isTrigger = true;
        obstacle.AddComponent<HazardObstacle>();
        if (travel != Vector3.zero)
        {
            var oscillator = obstacle.AddComponent<Oscillator>();
            oscillator.travel = travel;
            oscillator.frequency = movementSpeed;
        }
        Select(obstacle);
    }

    void AddRotatingHazard()
    {
        var obstacle = CreateHazardAt(placement, Vector3.zero, "Rotating Obstacle");
        obstacle.AddComponent<HazardRotator>().degreesPerSecond = 90f;
        Select(obstacle);
    }

    void AddWall(int side)
    {
        float x = side * 4.7f;
        var wall = CreateOutlinedObject(side < 0 ? "Left Boundary" : "Right Boundary", new Vector2(x, 5f), new Vector2(.35f, 12f), new Color(.12f, .22f, .35f));
        wall.AddComponent<ViewportSideWall>().side = side;
        Select(wall);
    }

    void CreateLevelTwo()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (string.IsNullOrEmpty(scene.path)) scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RemoveMissingScripts(scene);

        var level = FindAnyObjectByType<PivotLevel>();
        if (level == null) level = new GameObject("Level Controller").AddComponent<PivotLevel>();
        level.levelNumber = 2; level.levelTitle = "First Obstacle"; level.targetSeconds = 6; level.gemTotal = 4; level.minimumY = -.67f; level.maximumY = 11.5f;

        var camera = Camera.main;
        if (camera == null) camera = new GameObject("Portrait Camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 8.9f; camera.transform.position = new Vector3(0, 6, -10);
        camera.backgroundColor = new Color(.025f, .04f, .09f); camera.clearFlags = CameraClearFlags.SolidColor; camera.tag = "MainCamera";
        var player = FindAnyObjectByType<PivotPlayer>();
        if (player == null) player = CreatePlayer(level.transform, level);
        ConfigureLevelReferences(level, player);
        var follow = camera.GetComponent<HighWaterCamera>();
        if (follow == null) follow = camera.gameObject.AddComponent<HighWaterCamera>();
        follow.player = player;

        if (FindObjectsByType<ViewportSideWall>(FindObjectsSortMode.None).Length == 0)
        {
            CreateWall(-1, 6f, 12f);
            CreateWall(1, 6f, 12f);
        }
        if (FindObjectsByType<ViewportHorizontalBoundary>(FindObjectsSortMode.None).Length == 0)
        {
            CreateHorizontalBoundary("Bottom Boundary", -1f);
            CreateHorizontalBoundary("Top Boundary", 11.8f);
        }
        if (FindObjectsByType<GemPickup>(FindObjectsSortMode.None).Length == 0)
        {
            CreateGemAt(new Vector2(0f, 2.7f));
            CreateGemAt(new Vector2(-.7f, 4.4f));
            CreateGemAt(new Vector2(.7f, 6.5f));
            CreateGemAt(new Vector2(0f, 8.2f));
        }
        if (FindObjectsByType<HazardObstacle>(FindObjectsSortMode.None).Length == 0)
            CreateHazardAt(new Vector2(2.35f, 5.4f), Vector3.zero, "Static Red Obstacle");
        if (FindObjectsByType<SummitGoal>(FindObjectsSortMode.None).Length == 0)
            CreateSummitAt(new Vector2(0f, 10.2f), level);

        string levelPath = string.IsNullOrEmpty(scene.path) ? "Assets/Scenes/Levels/Level_02.unity" : scene.path;
        EditorSceneManager.SaveScene(scene, levelPath);
        AssetDatabase.Refresh();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        ShowNotification(new GUIContent("Level 2 created"));
    }

    void CreateLevelThree()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        const string levelPath = "Assets/Level_03.unity";

        var level = new GameObject("Level Controller").AddComponent<PivotLevel>();
        level.levelNumber = 3; level.levelTitle = "Narrow Turn"; level.targetSeconds = 9; level.gemTotal = 5; level.minimumY = -.67f; level.maximumY = 13.4f;

        var camera = new GameObject("Portrait Camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 8.9f; camera.transform.position = new Vector3(0, 7, -10);
        camera.backgroundColor = new Color(.025f, .04f, .09f); camera.clearFlags = CameraClearFlags.SolidColor; camera.tag = "MainCamera";

        var player = CreatePlayer(level.transform, level);
        var follow = camera.gameObject.AddComponent<HighWaterCamera>(); follow.player = player; follow.minimumY = 7.5f;
        CreateWall(-1, 7f, 14f); CreateWall(1, 7f, 14f);
        CreateHorizontalBoundary("Bottom Boundary", -1f); CreateHorizontalBoundary("Top Boundary", 13.7f);

        CreateGemAt(new Vector2(0f, 2.7f));
        CreateGemAt(new Vector2(-.8f, 4.35f));
        CreateGemAt(new Vector2(.75f, 6.15f));
        CreateGemAt(new Vector2(-.6f, 8.0f));
        CreateGemAt(new Vector2(.5f, 9.7f));
        CreateHazardAt(new Vector2(1.45f, 5.25f), Vector3.zero, "Static Red Obstacle");
        CreateSummitAt(new Vector2(0f, 12f), level);

        EditorSceneManager.SaveScene(scene, levelPath);
        AssetDatabase.Refresh();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        ShowNotification(new GUIContent("Level 3 created"));
    }

    void CreateLevelsFourToTen()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var levels = new[]
        {
            new LevelBlueprint(4, "First Sweep", 14f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.8f, 4.7f), new Vector2(.8f, 6.6f), new Vector2(-.6f, 8.8f), new Vector2(.6f, 11f) },
                new[] { new HazardSpec(new Vector2(0, 7.5f), Vector3.right * 2.6f, .45f, "Horizontal Sweeper") }),
            new LevelBlueprint(5, "Double Timing", 16f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.9f, 4.7f), new Vector2(.8f, 6.5f), new Vector2(-.8f, 8.7f), new Vector2(.8f, 10.7f), new Vector2(0, 13f) },
                new[] { new HazardSpec(new Vector2(1.4f, 5.6f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(0, 9.4f), Vector3.right * 2.8f, .55f, "Horizontal Sweeper") }),
            new LevelBlueprint(6, "Vertical Watch", 18f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.8f, 4.8f), new Vector2(.8f, 6.8f), new Vector2(-.8f, 9f), new Vector2(.8f, 11.2f), new Vector2(-.7f, 13.4f) },
                new[] { new HazardSpec(new Vector2(1.35f, 6f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(-1.3f, 10.2f), Vector3.up * 2.1f, .46f, "Vertical Sweeper") }),
            new LevelBlueprint(7, "Crossing Paths", 20f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.9f, 4.9f), new Vector2(.9f, 7f), new Vector2(-.9f, 9.2f), new Vector2(.9f, 11.5f), new Vector2(-.8f, 13.8f), new Vector2(.7f, 16f) },
                new[] { new HazardSpec(new Vector2(1.35f, 5.8f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(0, 10.1f), Vector3.right * 2.8f, .56f, "Horizontal Sweeper"), new HazardSpec(new Vector2(-1.2f, 14.2f), Vector3.up * 2.2f, .48f, "Vertical Sweeper") }),
            new LevelBlueprint(8, "Diagonal Drift", 22f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.9f, 4.9f), new Vector2(.9f, 7f), new Vector2(-.9f, 9.2f), new Vector2(.9f, 11.5f), new Vector2(-.9f, 13.8f), new Vector2(.9f, 16f) },
                new[] { new HazardSpec(new Vector2(1.35f, 5.8f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(0, 9.4f), Vector3.right * 2.8f, .58f, "Horizontal Sweeper"), new HazardSpec(new Vector2(-1.2f, 13f), Vector3.up * 2.2f, .5f, "Vertical Sweeper"), new HazardSpec(new Vector2(0, 16.2f), new Vector3(2.2f, 1.5f), .42f, "Diagonal Sweeper") }),
            new LevelBlueprint(9, "Timing Tower", 24f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.9f, 4.9f), new Vector2(.9f, 7f), new Vector2(-.9f, 9.2f), new Vector2(.9f, 11.5f), new Vector2(-.9f, 13.8f), new Vector2(.9f, 16f), new Vector2(0, 18.3f) },
                new[] { new HazardSpec(new Vector2(1.35f, 5.8f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(0, 8.8f), Vector3.right * 2.9f, .62f, "Horizontal Sweeper"), new HazardSpec(new Vector2(-1.2f, 12.1f), Vector3.up * 2.3f, .52f, "Vertical Sweeper"), new HazardSpec(new Vector2(0, 15.5f), new Vector3(2.3f, 1.5f), .45f, "Diagonal Sweeper"), new HazardSpec(new Vector2(1.2f, 18.4f), Vector3.up * 2.1f, .55f, "Vertical Sweeper") }),
            new LevelBlueprint(10, "Gauntlet", 26f,
                new[] { new Vector2(0, 2.8f), new Vector2(-.9f, 4.9f), new Vector2(.9f, 7f), new Vector2(-.9f, 9.2f), new Vector2(.9f, 11.5f), new Vector2(-.9f, 13.8f), new Vector2(.9f, 16f), new Vector2(-.9f, 18.3f), new Vector2(.8f, 20.6f) },
                new[] { new HazardSpec(new Vector2(1.35f, 5.8f), Vector3.zero, .6f, "Static Red Obstacle"), new HazardSpec(new Vector2(0, 8.4f), Vector3.right * 3f, .65f, "Horizontal Sweeper"), new HazardSpec(new Vector2(-1.2f, 11.4f), Vector3.up * 2.4f, .54f, "Vertical Sweeper"), new HazardSpec(new Vector2(0, 14.5f), new Vector3(2.4f, 1.6f), .48f, "Diagonal Sweeper"), new HazardSpec(new Vector2(1.15f, 17.7f), Vector3.up * 2.3f, .58f, "Vertical Sweeper"), new HazardSpec(new Vector2(0, 20.5f), Vector3.right * 3f, .68f, "Final Sweeper") })
        };

        foreach (var blueprint in levels) CreateAdvancedLevel(blueprint);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Level_04.unity", OpenSceneMode.Single);
        ShowNotification(new GUIContent("Levels 4 to 10 created"));
    }

    void CreateLevelsElevenToTwenty()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string[] titles = { "Spinner Steps", "Split Timing", "Turning Point", "Crosswind", "Spiral Ascent", "Pulse Run", "Switchback", "Redline", "Summit Circuit", "Final Climb" };
        var levels = new List<LevelBlueprint>();
        for (int stage = 0; stage < 10; stage++)
        {
            int number = 11 + stage;
            float goalY = 28f + stage * 2.25f;
            int gemCount = 9 + stage / 2;
            int hazardCount = 7 + stage;
            var gems = new Vector2[gemCount];
            var hazards = new HazardSpec[hazardCount];

            for (int index = 0; index < gemCount; index++)
            {
                float progress = index / (float)(gemCount - 1);
                float x = index == 0 ? 0f : (index % 2 == 0 ? .9f : -.9f);
                gems[index] = new Vector2(x, Mathf.Lerp(2.8f, goalY - 3.3f, progress));
            }

            for (int index = 0; index < hazardCount; index++)
            {
                float progress = index / (float)(hazardCount - 1);
                float y = Mathf.Lerp(5.1f, goalY - 4.6f, progress);
                int pattern = (index + stage) % 5;
                Vector2 position = new Vector2(((index + stage) % 3 - 1) * 1.2f, y);
                Vector3 travel = Vector3.zero;
                float frequency = .5f + stage * .025f;
                string name;
                float rotationSpeed = 0f;

                switch (pattern)
                {
                    case 0: position.x = index % 2 == 0 ? 1.55f : -1.55f; name = "Static Red Obstacle"; break;
                    case 1: position.x = 0f; travel = Vector3.right * (2.5f + stage * .05f); name = "Horizontal Sweeper"; break;
                    case 2: travel = Vector3.up * (2.0f + stage * .04f); name = "Vertical Sweeper"; break;
                    case 3: position.x = 0f; travel = new Vector3(2.0f + stage * .05f, 1.35f); name = "Diagonal Sweeper"; break;
                    default: name = "Rotating Sweeper"; rotationSpeed = (index % 2 == 0 ? 1f : -1f) * (80f + stage * 4f); break;
                }

                hazards[index] = new HazardSpec(position, travel, frequency, name, rotationSpeed);
            }

            levels.Add(new LevelBlueprint(number, titles[stage], goalY, gems, hazards));
        }

        foreach (var blueprint in levels) CreateAdvancedLevel(blueprint);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        EditorSceneManager.OpenScene("Assets/Level_11.unity", OpenSceneMode.Single);
        ShowNotification(new GUIContent("Levels 11 to 20 created"));
    }

    void CreateAdvancedLevel(LevelBlueprint blueprint)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var level = new GameObject("Level Controller").AddComponent<PivotLevel>();
        level.levelNumber = blueprint.number; level.levelTitle = blueprint.title; level.targetSeconds = 10 + blueprint.number; level.gemTotal = blueprint.gems.Length;
        level.minimumY = -.67f; level.maximumY = blueprint.goalY + .85f;

        var camera = new GameObject("Portrait Camera").AddComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 8.9f; camera.transform.position = new Vector3(0, 6, -10);
        camera.backgroundColor = new Color(.025f, .04f, .09f); camera.clearFlags = CameraClearFlags.SolidColor; camera.tag = "MainCamera";
        var player = CreatePlayer(level.transform, level);
        var follow = camera.gameObject.AddComponent<HighWaterCamera>(); follow.player = player; follow.minimumY = 6f;

        float topBoundary = blueprint.goalY + 1.2f;
        CreateWall(-1, topBoundary * .5f, topBoundary);
        CreateWall(1, topBoundary * .5f, topBoundary);
        CreateHorizontalBoundary("Bottom Boundary", -1f); CreateHorizontalBoundary("Top Boundary", topBoundary);
        foreach (var gem in blueprint.gems) CreateGemAt(gem);
        foreach (var hazard in blueprint.hazards)
        {
            var obstacle = CreateHazardAt(hazard.position, hazard.travel, hazard.name, hazard.frequency);
            if (hazard.rotationSpeed != 0f) obstacle.AddComponent<HazardRotator>().degreesPerSecond = hazard.rotationSpeed;
        }
        CreateSummitAt(new Vector2(0, blueprint.goalY), level);

        EditorSceneManager.SaveScene(scene, $"Assets/Level_{blueprint.number:00}.unity");
    }

    static PivotPlayer CreatePlayer(Transform parent, PivotLevel level)
    {
        var root = new GameObject("Pivot Player"); root.transform.SetParent(parent);
        var pivot = CreateRuntimeNode("Pivot Node", new Vector3(0, 1), new Color(.2f, .95f, 1f));
        var swing = CreateRuntimeNode("Swing Node", new Vector3(1.65f, -.67f), new Color(1f, .86f, .25f));
        pivot.AddComponent<CircleCollider2D>().isTrigger = true; swing.AddComponent<CircleCollider2D>().isTrigger = true;
        var pivotBody = pivot.AddComponent<Rigidbody2D>(); pivotBody.bodyType = RigidbodyType2D.Kinematic; pivotBody.useFullKinematicContacts = true;
        var swingBody = swing.AddComponent<Rigidbody2D>(); swingBody.bodyType = RigidbodyType2D.Kinematic; swingBody.useFullKinematicContacts = true;
        pivot.AddComponent<PivotNode>().level = level; swing.AddComponent<PivotNode>().level = level;
        var rod = root.AddComponent<LineRenderer>(); rod.positionCount = 2; rod.startWidth = rod.endWidth = .075f; rod.material = new Material(Shader.Find("Sprites/Default")); rod.startColor = rod.endColor = new Color(.75f, .9f, 1f);
        var player = root.AddComponent<PivotPlayer>(); player.pivot = pivot.transform; player.swingNode = swing.transform; player.rod = rod; player.level = level;
        return player;
    }

    static void ConfigureLevelReferences(PivotLevel level, PivotPlayer player)
    {
        player.level = level;
        foreach (var node in FindObjectsByType<PivotNode>(FindObjectsSortMode.None)) node.level = level;
        foreach (var summit in FindObjectsByType<SummitGoal>(FindObjectsSortMode.None)) summit.level = level;
    }

    static GameObject CreateRuntimeNode(string name, Vector3 position, Color color)
    {
        var node = new GameObject(name); node.transform.position = position; node.transform.localScale = Vector3.one * .44f;
        node.AddComponent<SpriteRenderer>().color = color; node.AddComponent<PivotVisual>();
        return node;
    }

    static void CreateWall(int side, float y, float height)
    {
        var wall = CreateOutlinedObject(side < 0 ? "Left Boundary" : "Right Boundary", new Vector2(side * 4.7f, y), new Vector2(.35f, height), new Color(.12f, .22f, .35f));
        wall.AddComponent<ViewportSideWall>().side = side;
    }

    static void CreateHorizontalBoundary(string name, float y)
    {
        var boundary = new GameObject(name);
        boundary.transform.position = new Vector3(0f, y, 0f);
        boundary.transform.localScale = new Vector3(10f, .22f, 1f);
        var collider = boundary.AddComponent<BoxCollider2D>(); collider.size = Vector2.one; collider.isTrigger = false;
        boundary.AddComponent<ViewportHorizontalBoundary>();
    }

    static void CreateGemAt(Vector2 position)
    {
        var gem = CreateOutlinedObject("Gem", position, new Vector2(.28f, .28f), new Color(.2f, 1f, .68f));
        gem.AddComponent<GemPickup>(); gem.AddComponent<CircleCollider2D>().isTrigger = true;
    }

    static GameObject CreateHazardAt(Vector2 position, Vector3 travel, string name, float frequency = .6f)
    {
        var hazard = CreateOutlinedObject(name, position, new Vector2(1.2f, .34f), new Color(.95f, .15f, .25f));
        hazard.AddComponent<BoxCollider2D>().isTrigger = true; hazard.AddComponent<HazardObstacle>();
        if (travel != Vector3.zero) { var oscillator = hazard.AddComponent<Oscillator>(); oscillator.travel = travel; oscillator.frequency = frequency; }
        return hazard;
    }

    static void CreateSummitAt(Vector2 position, PivotLevel level)
    {
        var summit = new GameObject("Summit Goal"); summit.transform.position = position; summit.transform.localScale = Vector3.one * 1.25f;
        summit.AddComponent<CircleCollider2D>().isTrigger = true; summit.AddComponent<SummitDotVisual>(); summit.AddComponent<SummitGoal>().level = level;
        var label = new GameObject("Summit Label"); label.transform.position = new Vector3(position.x, position.y + .92f, 0);
        var text = label.AddComponent<TextMesh>(); text.text = "SUMMIT"; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 46; text.characterSize = .12f; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = new Color(.25f, 1f, .58f);
    }

    static GameObject CreateOutlinedObject(string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Add " + name);
        go.transform.position = position;
        go.transform.localScale = size;
        var renderer = go.AddComponent<LineRenderer>();
        renderer.useWorldSpace = false; renderer.loop = true; renderer.positionCount = 4;
        renderer.SetPositions(new[] { new Vector3(-.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f), new Vector3(.5f, -.5f) });
        renderer.startWidth = renderer.endWidth = .055f;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
        renderer.startColor = renderer.endColor = color * 1.6f;
        EditorSceneManager.MarkSceneDirty(go.scene);
        return go;
    }

    static void Select(GameObject go)
    {
        Selection.activeGameObject = go;
        SceneView.lastActiveSceneView?.FrameSelected();
    }

    static void RemoveMissingScripts(UnityEngine.SceneManagement.Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects()) RemoveMissingScripts(root);
    }

    static void RemoveMissingScripts(GameObject gameObject)
    {
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        foreach (Transform child in gameObject.transform) RemoveMissingScripts(child.gameObject);
    }

    static void UpdateBuildSettings()
    {
        var scenes = new List<EditorBuildSettingsScene>();
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/MainMenu.unity") != null)
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true));
        scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Levels/Level_01.unity", true));
        for (int number = 2; number <= 20; number++)
        {
            string path = $"Assets/Level_{number:00}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null) scenes.Add(new EditorBuildSettingsScene(path, true));
        }
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/DailyChallenge.unity") != null)
            scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/DailyChallenge.unity", true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    readonly struct HazardSpec
    {
        public readonly Vector2 position; public readonly Vector3 travel; public readonly float frequency; public readonly string name; public readonly float rotationSpeed;
        public HazardSpec(Vector2 position, Vector3 travel, float frequency, string name, float rotationSpeed = 0f) { this.position = position; this.travel = travel; this.frequency = frequency; this.name = name; this.rotationSpeed = rotationSpeed; }
    }

    readonly struct LevelBlueprint
    {
        public readonly int number; public readonly string title; public readonly float goalY; public readonly Vector2[] gems; public readonly HazardSpec[] hazards;
        public LevelBlueprint(int number, string title, float goalY, Vector2[] gems, HazardSpec[] hazards) { this.number = number; this.title = title; this.goalY = goalY; this.gems = gems; this.hazards = hazards; }
    }
}
