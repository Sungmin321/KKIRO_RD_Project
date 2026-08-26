using UnityEngine;

namespace TDGame.Core
{
    public enum UnitGrade { Common = 1, Rare = 2, Epic = 3, Legendary = 4, Mythical = 5 }
    public enum DamageType { AD_Physical, AP_Magical }
    public enum AttackType { Melee, Ranged }
    public enum Tribe { None, Human, Beast, Undead, Elf, Mech, Dragon, Demon }

    [CreateAssetMenu(fileName = "NewUnitData", menuName = "TDGame/Unit Data")]
    public class UnitDataSO : ScriptableObject
    {
        [Header("1. 기본 정보 및 등급")]
        public string unitID;
        public string unitName;
        public UnitGrade grade = UnitGrade.Common;
        public Tribe tribe = Tribe.None;

        [Header("2. 공격 속성")]
        public DamageType damageType = DamageType.AD_Physical;
        public AttackType attackType = AttackType.Ranged;

        [Header("3. 전투 스탯")]
        public double baseDamage = 10.0;
        public float attackSpeed = 1.0f; // 초당 공격 횟수
        public float attackRange = 2.5f;

        [Header("4. 치명타")]
        [Range(0f, 1f)] public float criticalChance = 0.05f;
        public float criticalMultiplier = 1.5f;

        [Header("5. 유틸 / 방깎 / 스킬")]
        [Range(0f, 1f)] public float armorReductionRate = 0.0f; // 고정 방깎 비율 (예: 0.15 = 15%)
        [TextArea(2, 4)] public string skillDescription;
    }
}