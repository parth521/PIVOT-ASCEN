using UnityEngine;
namespace PivotAscent { public sealed class PivotNode : MonoBehaviour { public PivotLevel level; void OnTriggerEnter2D(Collider2D other) { if (other.GetComponent<GemPickup>() != null) { level.CollectGem(); Destroy(other.gameObject); } else if (other.GetComponent<HazardObstacle>() != null) level.Fail(); else if (other.GetComponent<SummitGoal>() != null) level.Complete(); } } }
