using UnityEngine;

namespace PivotAscent
{
    /// <summary>Compact procedural music and effects; no large audio files required.</summary>
    public sealed class GameAudio : MonoBehaviour
    {
        static GameAudio instance;
        AudioSource music;
        AudioSource effects;
        AudioClip gemChime;
        AudioClip obstacleHit;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Create() => Ensure();

        static void Ensure()
        {
            if (instance == null)
            {
                var root = new GameObject("PIVOT Audio");
                DontDestroyOnLoad(root);
                instance = root.AddComponent<GameAudio>();
            }

            // The audio object survives every scene load. Keeping the single
            // listener here means music works in the menu, all 20 levels, and
            // the runtime-generated Daily Challenge without duplicate listeners.
            if (FindAnyObjectByType<AudioListener>() == null) instance.gameObject.AddComponent<AudioListener>();
            AudioListener.pause = PlayerPrefs.GetInt("PivotMuted", 0) == 1;
        }

        // Scene-generated cameras (such as Daily Challenge) are created in
        // Awake. Waiting until Start prevents music from beginning before its
        // listener exists.
        void Start() => Setup();

        void Setup()
        {
            if (music != null) return;
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.volume = .14f;
            music.clip = BuildMusic();
            music.Play();

            effects = gameObject.AddComponent<AudioSource>();
            effects.volume = .7f;
            gemChime = BuildChime();
            obstacleHit = BuildImpact();
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        public static void PlayGem()
        {
            Ensure();
            if (instance.effects != null) instance.effects.PlayOneShot(instance.gemChime, .72f);
        }

        public static void PlayObstacleHit()
        {
            Ensure();
            if (instance.effects != null) instance.effects.PlayOneShot(instance.obstacleHit, .9f);
        }

        static AudioClip BuildMusic()
        {
            const int sampleRate = 24000;
            const float length = 12f;
            var samples = new float[(int)(sampleRate * length)];
            float[] notes = { 130.81f, 155.56f, 196f, 174.61f, 196f, 233.08f, 261.63f, 233.08f };
            for (int i = 0; i < samples.Length; i++)
            {
                float time = i / (float)sampleRate;
                int step = ((int)(time * 2f)) % notes.Length;
                float phase = time * 2f - Mathf.Floor(time * 2f);
                float envelope = Mathf.Clamp01(phase * 11f) * Mathf.Clamp01((1f - phase) * 3.5f);
                float pad = Mathf.Sin(time * Mathf.PI * 2f * (notes[step] * .5f)) * .13f;
                float lead = Mathf.Sin(time * Mathf.PI * 2f * notes[step]) * envelope * .11f;
                float shimmer = Mathf.Sin(time * Mathf.PI * 2f * notes[step] * 2f) * envelope * .025f;
                samples[i] = pad + lead + shimmer;
            }
            return MakeClip("Neon Ascent Loop", samples, sampleRate);
        }

        static AudioClip BuildChime()
        {
            const int sampleRate = 24000;
            var samples = new float[(int)(sampleRate * .18f)];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = i / (float)sampleRate;
                float frequency = Mathf.Lerp(780f, 1480f, time / .18f);
                samples[i] = Mathf.Sin(time * Mathf.PI * 2f * frequency) * Mathf.Exp(-time * 17f) * .55f;
            }
            return MakeClip("Gem Chime", samples, sampleRate);
        }

        static AudioClip BuildImpact()
        {
            const int sampleRate = 24000;
            var samples = new float[(int)(sampleRate * .26f)];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = i / (float)sampleRate;
                float noise = Mathf.PerlinNoise(i * .17f, .4f) * 2f - 1f;
                float tone = Mathf.Sin(time * Mathf.PI * 2f * Mathf.Lerp(150f, 55f, time / .26f));
                samples[i] = (noise * .5f + tone * .5f) * Mathf.Exp(-time * 13f) * .7f;
            }
            return MakeClip("Obstacle Impact", samples, sampleRate);
        }

        static AudioClip MakeClip(string name, float[] samples, int sampleRate)
        {
            var clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
