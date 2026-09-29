using System.Collections;
using UnityEngine;

namespace PivotAscent
{
    /// <summary>Small, reusable fade-and-rise entrance for runtime UI panels.</summary>
    [DisallowMultipleComponent]
    public sealed class UiEntranceAnimation : MonoBehaviour
    {
        [SerializeField] float duration = .22f;
        [SerializeField] float riseDistance = 22f;
        [SerializeField] float startScale = .94f;

        CanvasGroup canvasGroup;
        RectTransform rect;
        Vector2 restingPosition;
        Coroutine animation;

        void Awake()
        {
            rect = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        void OnEnable() => Play();

        public void Play()
        {
            if (!isActiveAndEnabled || rect == null || canvasGroup == null) return;
            restingPosition = rect.anchoredPosition;
            if (animation != null) StopCoroutine(animation);
            animation = StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            float elapsed = 0f;
            Vector2 startPosition = restingPosition - Vector2.up * riseDistance;
            Vector3 startSize = Vector3.one * startScale;
            canvasGroup.alpha = 0f;
            rect.anchoredPosition = startPosition;
            rect.localScale = startSize;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                canvasGroup.alpha = t;
                rect.anchoredPosition = Vector2.LerpUnclamped(startPosition, restingPosition, t);
                rect.localScale = Vector3.LerpUnclamped(startSize, Vector3.one, t);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            rect.anchoredPosition = restingPosition;
            rect.localScale = Vector3.one;
            animation = null;
        }

        void OnDisable()
        {
            if (animation != null) StopCoroutine(animation);
            animation = null;
            if (rect != null)
            {
                rect.anchoredPosition = restingPosition;
                rect.localScale = Vector3.one;
            }
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }
    }
}
