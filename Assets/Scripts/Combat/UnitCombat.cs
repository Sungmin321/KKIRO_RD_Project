using UnityEngine;
using TDGame.Core;

namespace TDGame.Combat
{
    public class UnitCombat : MonoBehaviour
    {
        [Header("유닛 데이터 SO")]
        public UnitDataSO unitData;

        [Header("실시간 계산 스탯 캐시")]
        public double calculatedDamage;
        public float calculatedAttackSpeed;

        private void Start()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradesChanged += RecalculateStats;

            RecalculateStats();
        }

        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
                UpgradeManager.Instance.OnUpgradesChanged -= RecalculateStats;
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

        /// <summary>
        /// 방깎 디버프 및 몬스터 방어력이 적용된 최종 실데미지 계산
        /// 방어력 공식: 데미지 경감률 = 100 / (100 + 방어력)
        /// </summary>
        public double CalculateDamage(float monsterBaseArmor, float teamTotalArmorReductionRate, out bool isCrit)
        {
            isCrit = Random.value < unitData.criticalChance;
            double dmg = calculatedDamage * (isCrit ? unitData.criticalMultiplier : 1.0);

            // 방어력 감소율 적용 (예: 100 방어력에 30% 방깎 -> 70 방어력)
            float effectiveArmor = Mathf.Max(0f, monsterBaseArmor * (1f - Mathf.Clamp01(teamTotalArmorReductionRate)));
            double armorMitigation = 100.0 / (100.0 + effectiveArmor);

            return dmg * armorMitigation;
        }

        public double GetDPS()
        {
            if (unitData == null) return 0;
            double avgCritMult = 1.0 + (unitData.criticalChance * (unitData.criticalMultiplier - 1.0));
            return calculatedDamage * calculatedAttackSpeed * avgCritMult;
        }
    }
}