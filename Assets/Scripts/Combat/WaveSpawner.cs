using System.Collections;
using UnityEngine;
using TDGame.Core;

namespace TDGame.Combat
{
    public class WaveSpawner : MonoBehaviour
    {
        public static WaveSpawner Instance { get; private set; }

        [Header("몬스터 프리팹 및 데이터")]
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private EnemyDataSO slimeData;

        [Header("이동 경로 (Waypoints)")]
        [SerializeField] private Transform[] waypoints;

        [Header("웨이브 진행 상태")]
        public float waveTimer = 20f;
        public bool isWaveRunning = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            StartCoroutine(GameWaveLoop());
        }

        /// <summary>
        /// 1라운드부터 10라운드(보스)까지 순차 진행되는 메인 게임 루프
        /// </summary>
        private IEnumerator GameWaveLoop()
        {
            while (GameManager.Instance != null && GameManager.Instance.currentRound <= 10)
            {
                int round = GameManager.Instance.currentRound;
                isWaveRunning = true;
                waveTimer = 20f;

                Debug.Log($"<color=cyan>====== [Round {round} 시작] ======</color>");

                if (round == 10)
                {
                    // 10라운드: 보스 1마리 소환
                    SpawnBoss(round);
                }
                else
                {
                    // 1~9라운드: 20초 동안 1초 간격으로 슬라임 20마리 소환
                    StartCoroutine(SpawnRegularWave(round));
                }

                // 20초 카운트다운 진행
                while (waveTimer > 0f)
                {
                    waveTimer -= Time.deltaTime;
                    yield return null;
                }

                isWaveRunning = false;

                // 다음 라운드로 전환
                GameManager.Instance.currentRound++;
                yield return new WaitForSeconds(1.0f); // 라운드 사이 1초 대기
            }

            Debug.Log("<color=green>모든 웨이브(10R) 종료!</color>");
        }

        private IEnumerator SpawnRegularWave(int round)
        {
            for (int i = 0; i < 20; i++)
            {
                SpawnMonster(round, false);
                yield return new WaitForSeconds(1.0f);
            }
        }

        private void SpawnBoss(int round)
        {
            Debug.Log("<color=red>⚠️ 보스 슬라임 출현! ⚠️</color>");
            GameObject bossObj = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);

            // 보스는 시각적으로 1.6배 크게 표시
            bossObj.transform.localScale = Vector3.one * 1.6f;

            EnemyController enemyComp = bossObj.GetComponent<EnemyController>();
            if (enemyComp != null)
            {
                enemyComp.Initialize(slimeData, round, true, waypoints);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentMonsterCount++;
            }
        }

        private void SpawnMonster(int round, bool isBoss)
        {
            if (enemyPrefab == null || waypoints == null || waypoints.Length == 0) return;

            GameObject enemyObj = Instantiate(enemyPrefab, waypoints[0].position, Quaternion.identity);
            EnemyController enemyComp = enemyObj.GetComponent<EnemyController>();

            if (enemyComp != null)
            {
                enemyComp.Initialize(slimeData, round, isBoss, waypoints);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentMonsterCount++;
            }
        }
    }
}