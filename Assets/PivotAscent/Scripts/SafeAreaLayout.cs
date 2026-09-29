using UnityEngine;

namespace PivotAscent
{
    /// <summary>Keeps the HUD clear of iPhone notches and Android system areas.</summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaLayout : MonoBehaviour
    {
        RectTransform rect;
        Rect lastArea;

        void OnEnable()
        {
            rect = GetComponent<RectTransform>();
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != lastArea) Apply();
        }

        void Apply()
        {
            if (rect == null) return;
            lastArea = Screen.safeArea;
            Vector2 minimum = lastArea.position;
            Vector2 maximum = lastArea.position + lastArea.size;
            minimum.x /= Screen.width; minimum.y /= Screen.height;
            maximum.x /= Screen.width; maximum.y /= Screen.height;
            rect.anchorMin = minimum;
            rect.anchorMax = maximum;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
        }
    }
}
