using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TDGame.Core;
using TDGame.Field;
using TDGame.Combat;

namespace TDGame.UI
{
    public class SummonUIController : MonoBehaviour
    {
        [Header("UI 텍스트 컴포넌트 연결")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI monsterCountText;
        [SerializeField] private TextMeshProUGUI roundTimerText; // 라운드 및 남은 시간 표시 슬롯
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
            // 1. 골드 및 몬스터 수치 업데이트
            if (GameManager.Instance != null)
            {
                if (goldText != null)
                    goldText.text = $"골드: {GameManager.Instance.currentGold:F0} G";

                if (monsterCountText != null)
                {
                    int count = GameManager.Instance.currentMonsterCount;
                    string color = count >= 80 ? "red" : (count >= 50 ? "yellow" : "white");
                    monsterCountText.text = $"몬스터: <color={color}>{count}</color> / {GameManager.MAX_MONSTER_LIMIT}";
                }
            }

            // 2. 라운드 및 웨이브 타이머 업데이트
            if (roundTimerText != null)
            {
                int currentRound = (GameManager.Instance != null) ? GameManager.Instance.currentRound : 1;
                float remainingTime = (WaveSpawner.Instance != null) ? Mathf.Max(0f, WaveSpawner.Instance.waveTimer) : 0f;

                if (currentRound > 10)
                {
                    roundTimerText.text = "<color=green>모든 웨이브 완료!</color>";
                }
                else if (currentRound == 10)
                {
                    roundTimerText.text = $"<color=red>[BOSS]</color> 10R ({remainingTime:F0}s)";
                }
                else
                {
                    roundTimerText.text = $"Round {currentRound} ({remainingTime:F0}s)";
                }
            }

            // 3. 소환 버튼 텍스트 업데이트
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