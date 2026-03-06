using UnityEngine;

namespace VRCarSimulator.Core
{
    /// <summary>
    /// Manages audio sources for engine, UI, environment, and feedback sounds.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource engineSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource environmentSource;
        [SerializeField] private AudioSource feedbackSource;

        [Header("Clips")]
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip errorClip;
        [SerializeField] private AudioClip warningClip;
        [SerializeField] private AudioClip clickClip;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlaySuccess() => PlayOneShot(feedbackSource, successClip);
        public void PlayError() => PlayOneShot(feedbackSource, errorClip);
        public void PlayWarning() => PlayOneShot(feedbackSource, warningClip);
        public void PlayClick() => PlayOneShot(uiSource, clickClip);

        public void SetEngineRPM(float normalizedRPM)
        {
            if (engineSource == null) return;
            engineSource.pitch = Mathf.Lerp(0.6f, 2.0f, normalizedRPM);
        }

        public void PlayOneShot(AudioSource source, AudioClip clip)
        {
            if (source != null && clip != null)
                source.PlayOneShot(clip);
        }
    }
}
