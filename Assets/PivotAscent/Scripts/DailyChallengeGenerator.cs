using System;
using UnityEngine;

namespace PivotAscent
{
    /// <summary>Builds one repeatable, date-seeded challenge each day.</summary>
    public sealed class DailyChallengeGenerator : MonoBehaviour
    {
        void Awake()
        {
            var random = new System.Random(DailyChallengeState.Seed);
            const float goalY = 17f;

            // The audio system and gameplay HUD both need a camera available
            // during scene startup, so create it before the generated level.
            var camera = new GameObject("Portrait Camera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.9f;
            camera.transform.position = new Vector3(0f, 6f, -10f);
            camera.backgroundColor = new Color(.025f, .04f, .09f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.tag = "MainCamera";
            // Normally the persistent GameAudio object owns the one listener.
            // This fallback also keeps the Daily scene playable in isolation.
            if (FindAnyObjectByType<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();

            var level = new GameObject("Daily Challenge Controller").AddComponent<PivotLevel>();
            level.levelNumber = 0;
            level.levelTitle = $"Daily • {DateTime.UtcNow:dd MMM}";
            level.targetSeconds = 18;
            level.gemTotal = 6;
            level.minimumY = -.67f;
            level.maximumY = goalY + .85f;
            level.isDailyChallenge = true;

            var player = CreatePlayer(level.transform, level);
            var follow = camera.gameObject.AddComponent<HighWaterCamera>();
            follow.player = player;
            follow.minimumY = 6f;

            CreateWall(-1, goalY * .5f, goalY + 1.2f);
            CreateWall(1, goalY * .5f, goalY + 1.2f);
            CreateBoundary("Bottom Boundary", -1f);
            CreateBoundary("Top Boundary", goalY + 1.2f);

            for (int gem = 0; gem < 6; gem++)
            {
                float y = 2.9f + gem * 2.05f;
                float x = gem == 0 ? 0f : (gem % 2 == 0 ? .85f : -.85f);
                CreateGem(new Vector2(x, y));
            }

            for (int obstacle = 0; obstacle < 4; obstacle++)
            {
                float y = 5.3f + obstacle * 2.65f;
                int style = random.Next(0, 4);
                Vector3 travel = style == 0 ? Vector3.zero : style == 1 ? Vector3.right * 2.4f : style == 2 ? Vector3.up * 1.6f : new Vector3(1.8f, 1.25f);
                float x = style == 0 ? (obstacle % 2 == 0 ? 1.55f : -1.55f) : 0f;
                CreateHazard(new Vector2(x, y), travel, .45f + random.Next(0, 20) * .01f);
            }

            CreateGoal(new Vector2(0f, goalY), level);
        }

        static PivotPlayer CreatePlayer(Transform parent, PivotLevel level)
        {
            var root = new GameObject("Pivot Player"); root.transform.SetParent(parent);
            var pivot = CreateNode("Pivot Node", new Vector3(0f, 1f), new Color(.2f, .95f, 1f));
            var swing = CreateNode("Swing Node", new Vector3(1.65f, -.67f), new Color(1f, .86f, .25f));
            foreach (var node in new[] { pivot, swing })
            {
                node.AddComponent<CircleCollider2D>().isTrigger = true;
                var body = node.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.useFullKinematicContacts = true;
                node.AddComponent<PivotNode>().level = level;
            }
            var rod = root.AddComponent<LineRenderer>();
            rod.positionCount = 2; rod.startWidth = rod.endWidth = .075f; rod.material = new Material(Shader.Find("Sprites/Default")); rod.startColor = rod.endColor = new Color(.75f, .9f, 1f) * 1.6f;
            var player = root.AddComponent<PivotPlayer>(); player.pivot = pivot.transform; player.swingNode = swing.transform; player.rod = rod; player.level = level;
            return player;
        }

        static GameObject CreateNode(string name, Vector3 position, Color color)
        {
            var node = new GameObject(name); node.transform.position = position; node.transform.localScale = Vector3.one * .44f;
            node.AddComponent<SpriteRenderer>().color = color; node.AddComponent<PivotVisual>();
            return node;
        }

        static void CreateWall(int side, float y, float height)
        {
            var wall = Outline(side < 0 ? "Left Boundary" : "Right Boundary", new Vector2(side * 4.7f, y), new Vector2(.35f, height), new Color(.12f, .22f, .35f));
            wall.AddComponent<ViewportSideWall>().side = side;
        }

        static void CreateBoundary(string name, float y)
        {
            var boundary = new GameObject(name); boundary.transform.position = new Vector3(0f, y); boundary.transform.localScale = new Vector3(10f, .22f, 1f);
            boundary.AddComponent<BoxCollider2D>().isTrigger = false; boundary.AddComponent<ViewportHorizontalBoundary>();
        }

        static void CreateGem(Vector2 position)
        {
            var gem = Outline("Gem", position, new Vector2(.28f, .28f), new Color(.2f, 1f, .68f));
            gem.AddComponent<GemPickup>(); gem.AddComponent<CircleCollider2D>().isTrigger = true;
        }

        static void CreateHazard(Vector2 position, Vector3 travel, float frequency)
        {
            var hazard = Outline("Daily Hazard", position, new Vector2(1.2f, .34f), new Color(.95f, .15f, .25f));
            hazard.AddComponent<BoxCollider2D>().isTrigger = false; hazard.AddComponent<HazardObstacle>();
            if (travel != Vector3.zero) { var oscillator = hazard.AddComponent<Oscillator>(); oscillator.travel = travel; oscillator.frequency = frequency; }
        }

        static void CreateGoal(Vector2 position, PivotLevel level)
        {
            var goal = new GameObject("Summit Goal"); goal.transform.position = position; goal.transform.localScale = Vector3.one * 1.25f;
            goal.AddComponent<CircleCollider2D>().isTrigger = true; goal.AddComponent<SummitDotVisual>(); goal.AddComponent<SummitGoal>().level = level;
            var label = new GameObject("Summit Label"); label.transform.position = new Vector3(position.x, position.y + .92f, 0f);
            var text = label.AddComponent<TextMesh>(); text.text = "DAILY SUMMIT"; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 42; text.characterSize = .11f; text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.color = new Color(.25f, 1f, .58f);
        }

        static GameObject Outline(string name, Vector2 position, Vector2 size, Color color)
        {
            var item = new GameObject(name); item.transform.position = position; item.transform.localScale = size;
            var line = item.AddComponent<LineRenderer>(); line.useWorldSpace = false; line.loop = true; line.positionCount = 4;
            line.SetPositions(new[] { new Vector3(-.5f, -.5f), new Vector3(-.5f, .5f), new Vector3(.5f, .5f), new Vector3(.5f, -.5f) });
            line.startWidth = line.endWidth = .055f; line.material = new Material(Shader.Find("Sprites/Default")); line.startColor = line.endColor = color * 1.6f;
            return item;
        }
    }
}
