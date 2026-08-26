using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TDGame.Core;
using TDGame.Field;

namespace TDGame.UI
{
    public class SummonUIController : MonoBehaviour
    {
        [Header("UI 텍스트 컴포넌트 연결")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI summonCostText;
        [SerializeField] private Button summonButton;

        private void Start()
        {
            if (summonButton != null)
            {
                summonButton.onClick.AddListener(OnSummonButtonClicked);
            }
            UpdateUI();
        }

        private void Update()
        {
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (GameManager.Instance != null && goldText != null)
            {
                goldText.text = $"골드: {GameManager.Instance.currentGold:F0} G";
            }

            if (TileGridManager.Instance != null && summonCostText != null)
            {
                double cost = TileGridManager.Instance.GetCurrentSummonCost();
                summonCostText.text = $"소환 ({cost:F0} G)";
            }
        }

        private void OnSummonButtonClicked()
        {
            if (TileGridManager.Instance != null)
            {
                TileGridManager.Instance.TrySummonUnit();
                UpdateUI();
            }
        }
    }
}