using UnityEngine;

/// <summary>Keeps a horizontal top or bottom boundary wide enough for every portrait aspect ratio.</summary>
public sealed class ViewportHorizontalBoundary : MonoBehaviour
{
    float height;

    void Awake() { height = transform.localScale.y; }

    void LateUpdate()
    {
        var camera = Camera.main;
        if (camera == null) return;
        float width = camera.orthographicSize * camera.aspect * 2f + .3f;
        transform.localScale = new Vector3(width, height, 1f);
    }
}
