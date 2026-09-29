using UnityEngine;

namespace PivotAscent
{
    /// <summary>Turns a red obstacle around its centre without changing its route.</summary>
    public sealed class HazardRotator : MonoBehaviour
    {
        [Tooltip("Degrees per second. A negative value turns counter-clockwise.")]
        public float degreesPerSecond = 90f;

        void Update() => transform.Rotate(0f, 0f, degreesPerSecond * Time.deltaTime);
    }
}
