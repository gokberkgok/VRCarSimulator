using UnityEngine;
using UnityEngine.UI;

namespace VRCarSimulator.UI
{
    /// <summary>
    /// Results screen shown after module/exam completion.
    /// Displays score breakdown and analytics summary.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Score Display")]
        [SerializeField] private Text totalScoreText;
        [SerializeField] private Text gradeText;
        [SerializeField] private Image scoreBar;

        [Header("Detail Rows")]
        [SerializeField] private Text[] metricLabels;
        [SerializeField] private Text[] metricValues;

        [Header("Buttons")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextModuleButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Pass/Fail")]
        [SerializeField] private float passingScore = 70f;
        [SerializeField] private Color passColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color failColor = new Color(0.8f, 0.2f, 0.2f);

        private void Start()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);
            if (nextModuleButton != null)
                nextModuleButton.onClick.AddListener(OnNextModule);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenu);
        }

        public void ShowResults(float totalScore, string[] labels, string[] values)
        {
            gameObject.SetActive(true);

            bool passed = totalScore >= passingScore;

            if (totalScoreText != null)
                totalScoreText.text = Mathf.RoundToInt(totalScore).ToString();

            if (gradeText != null)
            {
                gradeText.text = passed ? "GEÇTİ" : "KALDI";
                gradeText.color = passed ? passColor : failColor;
            }

            if (scoreBar != null)
            {
                scoreBar.fillAmount = totalScore / 100f;
                scoreBar.color = passed ? passColor : failColor;
            }

            // Populate detail metrics
            if (labels != null && metricLabels != null)
            {
                for (int i = 0; i < metricLabels.Length; i++)
                {
                    if (metricLabels[i] != null)
                        metricLabels[i].text = i < labels.Length ? labels[i] : "";
                }
            }

            if (values != null && metricValues != null)
            {
                for (int i = 0; i < metricValues.Length; i++)
                {
                    if (metricValues[i] != null)
                        metricValues[i].text = i < values.Length ? values[i] : "";
                }
            }
        }

        private void OnRetry()
        {
            int current = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentModuleIndex : 1;
            Core.GameManager.Instance?.StartModule(current);
        }

        private void OnNextModule()
        {
            int next = Core.GameManager.Instance != null ? Core.GameManager.Instance.CurrentModuleIndex + 1 : 1;
            Core.GameManager.Instance?.StartModule(next);
        }

        private void OnMainMenu()
        {
            Core.GameManager.Instance?.ReturnToMenu();
        }
    }
}
