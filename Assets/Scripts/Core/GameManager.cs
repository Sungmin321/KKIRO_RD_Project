using System;
using System.Collections.Generic;
using UnityEngine;
using TDGame.Utils;

namespace TDGame.Core
{
    public enum GameState { Lobby, InGame, Equipment, GameOver, Victory }

    [System.Serializable]
    public struct HPGrowthSegment
    {
        public int startRound;
        public int endRound;
        public float growthRate; // 0.12 = 12%
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const int MAX_MONSTER_LIMIT = 100;

        [Header("게임 진행 상태")]
        public int currentRound = 1;
        public int currentMonsterCount = 0;
        public double currentGold = 50.0;
        public int currentSeed = 0;

        [Header("소환 시스템")]
        public int summonCount = 0;
        [SerializeField] private double baseSummonCost = 10.0;
        [SerializeField] private double summonCostIncrease = 2.0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        /// <summary>
        /// 소환 횟수에 따라 점진적으로 증가하는 소환 비용 계산
        /// </summary>
        public double GetSummonCost()
        {
            return baseSummonCost + (summonCount * summonCostIncrease);
        }

        /// <summary>
        /// 라운드별 몬스터 최대 체력 계산
        /// </summary>
        public double GetEnemyMaxHP(int round, bool isBoss)
        {
            double baseHp = 100.0 * Mathf.Pow(1.15f, round - 1);
            return isBoss ? baseHp * 10.0 : baseHp;
        }

        /// <summary>
        /// 라운드별 몬스터 처치 골드 보상
        /// </summary>
        public double GetDropGold(int round, bool isBoss)
        {
            double baseGold = 2.0 + (round * 0.5);
            return isBoss ? baseGold * 5.0 : baseGold;
        }

        /// <summary>
        /// 보스 처치 시 씨앗 보상
        /// </summary>
        public int GetBossDropSeed(int round)
        {
            return 1;
        }
    }
}