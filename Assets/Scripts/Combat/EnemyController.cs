using UnityEngine;
using UnityEngine.UI;
using TDGame.Core;

namespace TDGame.Combat
{
    public class EnemyController : MonoBehaviour
    {
        public EnemyDataSO enemyData;
        public double currentHP;
        public double maxHP;
        public bool isBoss = false;

        [Header("체력바 UI")]
        [SerializeField] private Image hpFillImage;

        private Transform[] waypoints;
        private int currentWaypointIndex = 0;
        private float moveSpeed;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public void Initialize(EnemyDataSO data, int round, bool isBossMonster, Transform[] pathWaypoints)
        {
            enemyData = data;
            isBoss = isBossMonster;
            waypoints = pathWaypoints;
            currentWaypointIndex = 0;

            maxHP = GameManager.Instance.GetEnemyMaxHP(round, isBoss);
            currentHP = maxHP;
            moveSpeed = (data != null) ? data.moveSpeed : 1.5f;

            if (waypoints != null && waypoints.Length > 0)
            {
                transform.position = waypoints[0].position;
            }

            UpdateHPBar();
        }

        private void Update()
        {
            MoveAlongPath();
        }

        private void MoveAlongPath()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Transform targetPoint = waypoints[currentWaypointIndex];
            transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint.position) < 0.05f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        public void TakeDamage(double damage)
        {
            currentHP -= damage;
            UpdateHPBar();

            // 피격 시 빨갛게 깜빡임 효과
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                CancelInvoke(nameof(ResetColor));
                Invoke(nameof(ResetColor), 0.08f);
            }

            if (currentHP <= 0)
            {
                Die();
            }
        }

        private void ResetColor()
        {
            if (spriteRenderer != null) spriteRenderer.color = originalColor;
        }

        private void UpdateHPBar()
        {
            if (hpFillImage != null && maxHP > 0)
            {
                hpFillImage.fillAmount = (float)(currentHP / maxHP);
            }
        }

        private void Die()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentMonsterCount--;
                double dropGold = GameManager.Instance.GetDropGold(GameManager.Instance.currentRound, isBoss);
                GameManager.Instance.currentGold += dropGold;

                if (isBoss)
                {
                    int dropSeed = GameManager.Instance.GetBossDropSeed(GameManager.Instance.currentRound);
                    GameManager.Instance.currentSeed += dropSeed;
                }
            }

            Destroy(gameObject);
        }
    }
}