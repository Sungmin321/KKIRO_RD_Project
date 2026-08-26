using UnityEngine;
using TDGame.Core;

namespace TDGame.Combat
{
    public class UnitCombat : MonoBehaviour
    {
        [Header("유닛 데이터")]
        public UnitDataSO unitData;

        [Header("실시간 스탯 캐시")]
        public double calculatedDamage;
        public float calculatedAttackSpeed;

        [Header("마나 시스템")]
        public float currentMana = 0f;

        // 타일 1칸의 유니티 월드 크기 (1.3) + 외곽 여백 보정 상수
        private const float TILE_UNIT_SCALE = 1.3f;
        private const float EXTRA_MARGIN = 0.5f;

        private float attackCooldownTimer = 0f;
        private EnemyController currentTarget;

        /// <summary>
        /// 타일 단위를 유니티 월드 실제 거리로 환산
        /// </summary>
        public float GetRealWorldRange()
        {
            if (unitData == null) return 0f;
            // 1타일 사거리 = 1.3 * 1 + 0.5 = 1.8 유닛 (가장자리 타일에서 트랙의 슬라임을 타격 가능)
            // 4타일 사거리 = 1.3 * 4 + 0.5 = 5.7 유닛 (먼 거리 전체 커버)
            return (unitData.attackRange * TILE_UNIT_SCALE) + EXTRA_MARGIN;
        }

        private void Start()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradesChanged += RecalculateStats;

            RecalculateStats();
            currentMana = 0f;
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradesChanged -= RecalculateStats;
        }

        private void Update()
        {
            if (unitData == null) return;

            // 1. 마나 자연 회복
            if (unitData.maxMana > 0f && currentMana < unitData.maxMana)
            {
                currentMana += unitData.manaRegenPerSec * Time.deltaTime;
                if (currentMana >= unitData.maxMana)
                {
                    currentMana = unitData.maxMana;
                    TryCastActiveSkill();
                }
            }

            // 2. 공격 쿨다운 처리
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= Time.deltaTime;
            }

            // 3. 타겟 탐색 및 평타 공격
            FindTarget();
            if (currentTarget != null && attackCooldownTimer <= 0f)
            {
                ExecuteAttack(currentTarget);
                attackCooldownTimer = 1.0f / Mathf.Max(0.1f, calculatedAttackSpeed);
            }
        }

        private void FindTarget()
        {
            float realRange = GetRealWorldRange();

            // 기존 타겟이 유효 사거리 내에 있는지 검사
            if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.transform.position) <= realRange)
            {
                return;
            }

            currentTarget = null;
            EnemyController[] allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            float minDistance = float.MaxValue;

            foreach (var enemy in allEnemies)
            {
                if (enemy == null) continue;

                float dist = Vector3.Distance(transform.position, enemy.transform.position);
                if (dist <= realRange && dist < minDistance)
                {
                    minDistance = dist;
                    currentTarget = enemy;
                }
            }
        }

        private void ExecuteAttack(EnemyController target)
        {
            bool isCrit;
            bool isSkillTriggered;
            float targetArmor = (target.enemyData != null) ? target.enemyData.baseArmor : 0f;

            double finalDamage = PerformAttack(targetArmor, 0f, out isCrit, out isSkillTriggered);
            target.TakeDamage(finalDamage);

            // 공격 피드백 시각 라인 표시
            Debug.DrawLine(transform.position, target.transform.position, isCrit ? Color.red : Color.yellow, 0.15f);
        }

        public void RecalculateStats()
        {
            if (unitData == null) return;

            float gradeMultiplier = (UpgradeManager.Instance != null)
                ? UpgradeManager.Instance.GetGradeUpgradeMultiplier(unitData.grade) : 1f;

            double seedMultiplier = (unitData.grade == UnitGrade.Mythical && UpgradeManager.Instance != null)
                ? UpgradeManager.Instance.GetMythicalSeedBonus() : 1.0;

            calculatedDamage = unitData.baseDamage * gradeMultiplier * seedMultiplier;
            calculatedAttackSpeed = unitData.attackSpeed;
        }

        public double PerformAttack(float monsterArmor, float teamArmorReductionRate, out bool isCrit, out bool isSkillTriggered)
        {
            isSkillTriggered = false;
            isCrit = Random.value < unitData.criticalChance;
            double baseDmg = calculatedDamage * (isCrit ? unitData.criticalMultiplier : 1.0);

            AddMana(unitData.manaGainOnAttack);

            double finalDmg = baseDmg;
            float effectiveArmor = Mathf.Max(0f, monsterArmor * (1f - Mathf.Clamp01(teamArmorReductionRate)));

            switch (unitData.unitID)
            {
                case "UNIT_GOMI":
                    if (Random.value < 0.15f)
                    {
                        isSkillTriggered = true;
                        return finalDmg;
                    }
                    break;

                case "UNIT_TORI":
                    if (Random.value < 0.20f)
                    {
                        isSkillTriggered = true;
                        finalDmg *= 1.5;
                    }
                    break;
            }

            double armorMitigation = 100.0 / (100.0 + effectiveArmor);
            return finalDmg * armorMitigation;
        }

        public void AddMana(float amount)
        {
            if (unitData == null || unitData.maxMana <= 0f) return;

            currentMana = Mathf.Min(unitData.maxMana, currentMana + amount);
            if (currentMana >= unitData.maxMana)
            {
                TryCastActiveSkill();
            }
        }

        private void TryCastActiveSkill()
        {
            currentMana = 0f;
            Debug.Log($"<color=yellow>[액티브 스킬 시전]</color> {unitData.unitName} 발동!");
        }

        // Scene 뷰에서 유닛 선택 시 실제 사거리를 원형 와이어로 표시
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, GetRealWorldRange());
        }
    }
}