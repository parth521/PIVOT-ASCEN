using UnityEngine;
namespace PivotAscent { public sealed class Oscillator : MonoBehaviour { public Vector3 travel = Vector3.right * 2f; public float frequency = 1f; Vector3 origin; void Start() { origin = transform.position; } void Update() { transform.position = origin + travel * Mathf.Sin(Time.time * frequency * Mathf.PI * 2f) * .5f; } } }
