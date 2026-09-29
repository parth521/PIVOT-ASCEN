using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PivotAscent
{
    /// <summary>
    /// Enables a small bloom pass for the cyan, green, and red gameplay lines.
    /// It is intentionally limited to a single mobile-friendly post-process effect.
    /// </summary>
    public static class MobileNeonCamera
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            SceneManager.sceneLoaded -= SetupScene;
            SceneManager.sceneLoaded += SetupScene;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void SetupActiveScene() => SetupScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);

        static void SetupScene(Scene scene, LoadSceneMode mode)
        {
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                camera.allowHDR = true;
                var data = camera.GetComponent<UniversalAdditionalCameraData>();
                if (data == null) data = camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
            }
            EnsureBloomVolume();
        }

        static void EnsureBloomVolume()
        {
            if (GameObject.Find("Mobile Neon Bloom") != null) return;
            var volumeObject = new GameObject("Mobile Neon Bloom");
            Object.DontDestroyOnLoad(volumeObject);
            var volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 100f;
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
            var bloom = volume.profile.Add<Bloom>(true);
            bloom.threshold.value = .82f;
            bloom.intensity.value = .48f;
            bloom.scatter.value = .68f;
            bloom.maxIterations.value = 3;
            bloom.highQualityFiltering.value = false;
        }
    }
}
