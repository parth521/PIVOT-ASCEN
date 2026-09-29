using UnityEngine;

namespace PivotAscent
{
    public sealed class HazardObstacle : MonoBehaviour
    {
        Collider2D obstacleCollider;

        void Awake() => ConfigureCollider();

        void ConfigureCollider()
        {
            obstacleCollider = GetComponent<Collider2D>();
            obstacleCollider.isTrigger = false;
            if (obstacleCollider is BoxCollider2D box) box.size = Vector2.one;
        }

        // Pivot nodes are positioned directly by the movement controller.  Test the
        // swept path ourselves so fast movement cannot skip through a thin red bar.
        public bool TouchesPath(Vector2 start, Vector2 end, float radius)
        {
            if (obstacleCollider == null) obstacleCollider = GetComponent<Collider2D>();
            if (obstacleCollider == null || !obstacleCollider.enabled) return false;

            Bounds bounds = obstacleCollider.bounds;
            bounds.Expand(radius * 2f);
            return SegmentTouchesBounds(start, end, bounds);
        }

        static bool SegmentTouchesBounds(Vector2 start, Vector2 end, Bounds bounds)
        {
            if (bounds.Contains(start) || bounds.Contains(end)) return true;
            Vector2 delta = end - start;
            float enter = 0f, exit = 1f;
            if (!ClipAxis(start.x, delta.x, bounds.min.x, bounds.max.x, ref enter, ref exit)) return false;
            return ClipAxis(start.y, delta.y, bounds.min.y, bounds.max.y, ref enter, ref exit);
        }

        static bool ClipAxis(float start, float delta, float minimum, float maximum, ref float enter, ref float exit)
        {
            if (Mathf.Abs(delta) < .0001f) return start >= minimum && start <= maximum;
            float a = (minimum - start) / delta;
            float b = (maximum - start) / delta;
            if (a > b) { float swap = a; a = b; b = swap; }
            enter = Mathf.Max(enter, a);
            exit = Mathf.Min(exit, b);
            return enter <= exit;
        }

        void OnCollisionEnter2D(Collision2D collision) => ResetPivotLevel(collision.collider);
        void OnTriggerEnter2D(Collider2D other) => ResetPivotLevel(other);

        static void ResetPivotLevel(Component other)
        {
            var node = other.GetComponent<PivotNode>();
            if (node != null && node.level != null) node.level.Fail();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (GetComponent<Collider2D>() != null) ConfigureCollider();
        }
#endif
    }
}
