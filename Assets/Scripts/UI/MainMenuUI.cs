using UnityEngine;
using UnityEngine.UI;

namespace VRCarSimulator.UI
{
    /// <summary>
    /// Main menu controller – module selection, settings, exam mode entry.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject moduleSelectPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject profilePanel;

        [Header("Module Buttons")]
        [SerializeField] private Button[] moduleButtons;

        [Header("Navigation")]
        [SerializeField] private Button startExamButton;
        [SerializeField] private Button freeDriveButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button profileButton;
        [SerializeField] private Button backButton;

        [Header("Animation")]
        [SerializeField] private float panelTransitionDuration = 0.3f;

        private GameObject currentPanel;

        private void Start()
        {
            ShowPanel(mainPanel);

            if (startExamButton != null)
                startExamButton.onClick.AddListener(() => Core.GameManager.Instance?.StartExamMode());

            if (freeDriveButton != null)
                freeDriveButton.onClick.AddListener(() => Core.GameManager.Instance?.StartFreeDrive());

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));

            if (profileButton != null)
                profileButton.onClick.AddListener(() => ShowPanel(profilePanel));

            if (backButton != null)
                backButton.onClick.AddListener(() => ShowPanel(mainPanel));

            SetupModuleButtons();
        }

        private void SetupModuleButtons()
        {
            if (moduleButtons == null) return;
            for (int i = 0; i < moduleButtons.Length; i++)
            {
                int moduleIndex = i + 1;
                if (moduleButtons[i] != null)
                    moduleButtons[i].onClick.AddListener(() => Core.GameManager.Instance?.StartModule(moduleIndex));
            }
        }

        private void ShowPanel(GameObject panel)
        {
            if (currentPanel != null) currentPanel.SetActive(false);
            if (panel != null) panel.SetActive(true);
            currentPanel = panel;
        }

        public void ShowModuleSelect()
        {
            ShowPanel(moduleSelectPanel);
        }
    }
}
