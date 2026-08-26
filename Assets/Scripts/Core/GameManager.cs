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

        [Header("게임 상태")]
        public GameState currentState = GameState.InGame;

        [Header("재화 및 경제 설정")]
        public double currentGold = 100.0;
        public int currentSeed = 0;
        public int currentRound = 1;

        [Header("체력 밸런스 설정 (구간별 가속 복리)")]
        [SerializeField] private double baseMonsterHP = 100.0;
        [SerializeField]
        private List<HPGrowthSegment> hpSegments = new List<HPGrowthSegment>()
        {
            new HPGrowthSegment { startRound = 1, endRound = 20, growthRate = 0.12f },
            new HPGrowthSegment { startRound = 21, endRound = 40, growthRate = 0.16f },
            new HPGrowthSegment { startRound = 41, endRound = 80, growthRate = 0.22f }
        };

        [Header("보스 체력 배율")]
        [SerializeField] private double earlyBossMultiplier = 50.0; // 10, 20, 30, 40R
        [SerializeField] private double lateBossMultiplier = 100.0; // 50, 60, 70, 80R

        [Header("패배 조건 카운터")]
        public int currentMonsterCount = 0;
        public const int MAX_MONSTER_LIMIT = 100;
        public float currentBossTimer = 60f;
        public bool isBossWave = false;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            RunBalanceSimulation();
        }

        private void Update()
        {
            if (currentState != GameState.InGame) return;

            // 패배 조건 1: 필드 몬스터 100마리 초과
            if (currentMonsterCount >= MAX_MONSTER_LIMIT)
            {
                TriggerGameOver("필드에 몬스터가 100마리 이상 누적되었습니다.");
            }

            // 패배 조건 2: 보스 타임오버 (60초)
            if (isBossWave)
            {
                currentBossTimer -= Time.deltaTime;
                if (currentBossTimer <= 0f)
                {
                    TriggerGameOver("보스를 60초 내에 처치하지 못했습니다.");
                }
            }
        }

        public void TriggerGameOver(string reason)
        {
            currentState = GameState.GameOver;
            Debug.LogError($"<color=red>[GAME OVER]</color> {reason}");
        }

        public void ChangeGameState(GameState newState)
        {
            currentState = newState;
            Debug.Log($"화면 상태 전환: {newState}");
        }

        /// <summary>
        /// 구간별 복리 성장 공식을 적용한 최종 몬스터 HP 계산
        /// </summary>
        public double GetEnemyMaxHP(int round, bool isBoss)
        {
            double hp = baseMonsterHP;

            for (int r = 1; r < round; r++)
            {
                float rate = 0.12f;
                foreach (var seg in hpSegments)
                {
                    if (r >= seg.startRound && r <= seg.endRound)
                    {
                        rate = seg.growthRate;
                        break;
                    }
                }
                hp *= (1.0 + rate);
            }

            if (isBoss)
            {
                double bossMult = (round <= 40) ? earlyBossMultiplier : lateBossMultiplier;
                hp *= bossMult;
            }

            return hp;
        }

        /// <summary>
        /// 10스테이지 구간별 드랍 골드 (소환 인플레이션 억제용 선형 증가)
        /// </summary>
        public double GetDropGold(int round, bool isBoss)
        {
            int tier = (round - 1) / 10; // 0 ~ 7
            double baseDrop = 1.0 + (tier * 0.5); // 일반몹: 1G ~ 4.5G

            if (isBoss) return (tier + 1) * 30.0; // 보스: 30G ~ 240G
            return baseDrop;
        }

        /// <summary>
        /// 보스 처치 시 지급되는 씨앗 수량
        /// </summary>
        public int GetBossDropSeed(int round)
        {
            int tier = round / 10;
            return Mathf.Max(1, tier); // 10R: 1개, 80R: 8개
        }

        [ContextMenu("80라운드 밸런스 시뮬레이션 실행")]
        public void RunBalanceSimulation()
        {
            Debug.Log("====================== [운빨존많겜 80R 밸런스 시뮬레이션] ======================");
            for (int r = 10; r <= 80; r += 10)
            {
                double normalHP = GetEnemyMaxHP(r, false);
                double bossHP = GetEnemyMaxHP(r, true);

                // 웨이브 클리어 요구 DPS (일반몹: 20마리 총 체력 / 20초, 보스몹: 보스 체력 / 60초)
                double requiredWaveDPS = (normalHP * 20.0) / 20.0;
                double requiredBossDPS = bossHP / 60.0;
                double dropGold = GetDropGold(r, false);
                double bossGold = GetDropGold(r, true);
                int bossSeed = GetBossDropSeed(r);

                Debug.Log($"[R{r:D2}] 일반HP: {FormatUtil.FormatNumber(normalHP)} (요구DPS: {FormatUtil.FormatNumber(requiredWaveDPS)}) | " +
                          $"보스HP: {FormatUtil.FormatNumber(bossHP)} (요구DPS: {FormatUtil.FormatNumber(requiredBossDPS)}) | " +
                          $"보상: 일반 {dropGold:F1}G / 보스 {bossGold:F0}G + 씨앗 {bossSeed}개");
            }
            Debug.Log("=================================================================================");
        }
    }
}