using UnityEngine;
public sealed class ViewportSideWall : MonoBehaviour { public int side = 1; void LateUpdate() { var camera = Camera.main; if (camera == null) return; float x = camera.ViewportToWorldPoint(new Vector3(side < 0 ? .035f : .965f, .5f, Mathf.Abs(camera.transform.position.z))).x; transform.position = new Vector3(x, transform.position.y, transform.position.z); } }
