using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRCarSimulator.Core
{
    /// <summary>
    /// Handles async scene loading with progress callbacks.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public event System.Action<float> OnLoadProgress;
        public event System.Action OnLoadComplete;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneAsync(sceneName));
        }

        public void LoadScene(int sceneIndex)
        {
            StartCoroutine(LoadSceneAsync(sceneIndex));
        }

        private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            yield return TrackProgress(op);
        }

        private System.Collections.IEnumerator LoadSceneAsync(int sceneIndex)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneIndex);
            yield return TrackProgress(op);
        }

        private System.Collections.IEnumerator TrackProgress(AsyncOperation op)
        {
            while (!op.isDone)
            {
                OnLoadProgress?.Invoke(op.progress);
                yield return null;
            }
            OnLoadComplete?.Invoke();
        }
    }
}
