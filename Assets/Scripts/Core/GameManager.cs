using UnityEngine;

namespace VRCarSimulator.Core
{
    /// <summary>
    /// Singleton GameManager – controls game state, module flow, and global references.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private int currentModuleIndex = -1;

        public GameState CurrentState => currentState;
        public int CurrentModuleIndex => currentModuleIndex;

        public event System.Action<GameState> OnStateChanged;
        public event System.Action<int> OnModuleChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            OnStateChanged?.Invoke(currentState);
        }

        public void StartModule(int moduleIndex)
        {
            currentModuleIndex = moduleIndex;
            SetState(GameState.InModule);
            OnModuleChanged?.Invoke(moduleIndex);
        }

        public void CompleteModule()
        {
            SetState(GameState.ModuleComplete);
        }

        public void ReturnToMenu()
        {
            currentModuleIndex = -1;
            SetState(GameState.MainMenu);
        }

        public void StartExamMode()
        {
            SetState(GameState.ExamMode);
        }

        public void StartFreeDrive()
        {
            currentModuleIndex = 5;
            SetState(GameState.FreeDrive);
        }

        public void PauseGame()
        {
            SetState(GameState.Paused);
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            Time.timeScale = 1f;
            SetState(GameState.InModule);
        }
    }

    public enum GameState
    {
        MainMenu,
        InModule,
        ModuleComplete,
        ExamMode,
        FreeDrive,
        Paused,
        Results
    }
}
