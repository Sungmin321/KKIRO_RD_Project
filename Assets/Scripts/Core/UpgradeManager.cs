using System;
using System.Collections.Generic;
using UnityEngine;

namespace TDGame.Core
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }
        public event Action OnUpgradesChanged;

        [Header("골드 등급별 강화 레벨")]
        private Dictionary<UnitGrade, int> gradeUpgradeLevels = new Dictionary<UnitGrade, int>();

        [Header("씨앗 신화 강화 수치")]
        public int mythicSeedUpgradeLevel = 0;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            foreach (UnitGrade grade in Enum.GetValues(typeof(UnitGrade)))
            {
                gradeUpgradeLevels[grade] = 0;
            }
        }

        public float GetGradeUpgradeMultiplier(UnitGrade grade)
        {
            int lv = gradeUpgradeLevels.ContainsKey(grade) ? gradeUpgradeLevels[grade] : 0;
            return 1.0f + (lv * 0.10f); // 강화당 10% 증가
        }

        public double GetMythicalSeedBonus()
        {
            return 1.0 + (mythicSeedUpgradeLevel * 0.15); // 씨앗 강화당 신화 데미지 +15%
        }

        /// <summary>
        /// 골드를 소모하여 특정 등급 전체 강화
        /// </summary>
        public bool TryUpgradeGradeWithGold(UnitGrade grade, double cost)
        {
            if (GameManager.Instance.currentGold < cost) return false;

            GameManager.Instance.currentGold -= cost;
            gradeUpgradeLevels[grade]++;
            OnUpgradesChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 씨앗을 소모해 신화 공격력 영구 강화
        /// </summary>
        public bool TryUpgradeMythicWithSeed(int seedCost)
        {
            if (GameManager.Instance.currentSeed < seedCost) return false;

            GameManager.Instance.currentSeed -= seedCost;
            mythicSeedUpgradeLevel++;
            OnUpgradesChanged?.Invoke();
            return true;
        }
    }
}