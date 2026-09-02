using UnityEngine;

namespace TDGame.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const int MAX_MONSTER_LIMIT = 100;

        [Header("게임 진행 상태")]
        public int currentRound = 1;
        [SerializeField] private int _currentMonsterCount = 0; // 프라이빗으로 변경
        public double currentGold = 50.0;
        public int currentSeed = 0;
        public bool isGameOver = false; // 게임 오버 상태 추가

        // 외부에서 몬스터 수를 수정할 때 게임 오버 체크를 위한 프로퍼티
        public int currentMonsterCount
        {
            get => _currentMonsterCount;
            set
            {
                _currentMonsterCount = value;
                CheckGameOver(); // 수치가 바뀔 때마다 체크
            }
        }

        [Header("소환 시스템")]
        public int summonCount = 0;
        [SerializeField] private double baseSummonCost = 10.0;
        [SerializeField] private double summonCostIncrease = 2.0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            isGameOver = false;
            Time.timeScale = 1f; // 시작 시 시간 정상화
        }

        /// <summary>
        /// 몬스터 수가 100마리를 넘었는지 확인
        /// </summary>
        private void CheckGameOver()
        {
            if (isGameOver) return;

            if (_currentMonsterCount >= MAX_MONSTER_LIMIT)
            {
                isGameOver = true;
                Time.timeScale = 0f; // 게임 일시 정지
                Debug.Log("<color=red><b>[GAME OVER]</b></color> 몬스터가 100마리에 도달했습니다!");
                // 추후 이곳에 게임 오버 UI 팝업 로직 추가
            }
        }

        public double GetSummonCost()
        {
            return baseSummonCost + (summonCount * summonCostIncrease);
        }

        public double GetEnemyMaxHP(int round, bool isBoss)
        {
            double baseHp = 100.0 * Mathf.Pow(1.15f, round - 1);
            return isBoss ? baseHp * 10.0 : baseHp;
        }

        public double GetDropGold(int round, bool isBoss)
        {
            double baseGold = 2.0 + (round * 0.5);
            return isBoss ? baseGold * 5.0 : baseGold;
        }

        public int GetBossDropSeed(int round)
        {
            return 1;
        }
    }
}