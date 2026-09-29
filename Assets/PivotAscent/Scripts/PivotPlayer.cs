using UnityEngine;
namespace PivotAscent
{
    public sealed class PivotPlayer : MonoBehaviour
    {
        public Transform pivot; public Transform swingNode; public LineRenderer rod; public PivotLevel level;
        public float rodLength = 2.35f; public float angularSpeed = 165f;
        float angle = -45f; int direction = 1;

        void Update()
        {
            if (level == null || pivot == null || swingNode == null || rod == null)
            {
                enabled = false;
                return;
            }
            if (level != null && level.IsComplete) return;
            if (PivotInput.TapDown) SwapPivot();
            KeepPivotInView();
            float nextAngle = angle + direction * angularSpeed * Time.deltaTime;
            Vector3 next = SwingPosition(nextAngle);
            if (!InsideViewport(next))
            {
                direction *= -1;
                nextAngle = angle + direction * angularSpeed * Time.deltaTime;
                next = SwingPosition(nextAngle);
            }
            next = ClampToLevelBounds(ClampToViewport(next));
            if (HitsObstacle(swingNode.position, next))
            {
                level.Fail();
                return;
            }
            angle = nextAngle;
            swingNode.position = next;
            rod.SetPosition(0, pivot.position); rod.SetPosition(1, swingNode.position);
            CheckContacts(pivot.position); CheckContacts(swingNode.position);
        }

        void SwapPivot() { Vector3 old = pivot.position; pivot.position = swingNode.position; swingNode.position = old; direction *= -1; angle = Mathf.Atan2(swingNode.position.y - pivot.position.y, swingNode.position.x - pivot.position.x) * Mathf.Rad2Deg; }

        bool HitsObstacle(Vector3 start, Vector3 end)
        {
            foreach (var hazard in FindObjectsByType<HazardObstacle>(FindObjectsSortMode.None))
                if (hazard.TouchesPath(start, end, .24f)) return true;
            return false;
        }

        void CheckContacts(Vector3 point)
        {
            foreach (var hazard in FindObjectsByType<HazardObstacle>(FindObjectsSortMode.None))
                if (hazard.TouchesPath(point, point, .24f)) { level.Fail(); return; }

            foreach (var hit in Physics2D.OverlapCircleAll(point, .24f))
            {
                if (hit.GetComponent<GemPickup>() != null) { level.CollectGem(); Destroy(hit.gameObject); }
                else if (hit.GetComponent<SummitGoal>() != null) level.Complete();
            }
        }
        Vector3 SwingPosition(float targetAngle) { float radians = targetAngle * Mathf.Deg2Rad; return pivot.position + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians)) * rodLength; }
        bool InsideViewport(Vector3 point) { var camera = Camera.main; if (camera == null) return true; Vector3 view = camera.WorldToViewportPoint(point); return view.x >= .055f && view.x <= .945f && view.y >= .055f && view.y <= .945f; }
        Vector3 ClampToViewport(Vector3 point) { var camera = Camera.main; if (camera == null) return point; Vector3 view = camera.WorldToViewportPoint(point); view.x = Mathf.Clamp(view.x, .055f, .945f); view.y = Mathf.Clamp(view.y, .055f, .945f); return camera.ViewportToWorldPoint(new Vector3(view.x, view.y, Mathf.Abs(camera.transform.position.z))); }
        Vector3 ClampToLevelBounds(Vector3 point)
        {
            if (level == null) return point;
            point.y = Mathf.Clamp(point.y, level.minimumY, level.maximumY);
            return point;
        }

        void KeepPivotInView() { pivot.position = ClampToLevelBounds(ClampToViewport(pivot.position)); }
    }
}
