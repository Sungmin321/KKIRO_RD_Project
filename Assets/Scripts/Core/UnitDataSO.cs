using UnityEngine;

namespace TDGame.Core
{
    public enum UnitGrade
    {
        Common,
        Uncommon,
        Rare,
        Ancient,
        Mythical
    }

    public enum Tribe
    {
        Beast,
        Mechanical,
        Elemental,
        Abyssal,
        Dragon
    }

    public enum AttackType
    {
        Single,
        Splash
    }

    [CreateAssetMenu(fileName = "UnitData_", menuName = "TDGame/Unit Data", order = 1)]
    public class UnitDataSO : ScriptableObject
    {
        [Header("1. 기본 정보 및 외형")]
        public string unitID;
        public string unitName;
        public Sprite unitSprite;
        public UnitGrade grade = UnitGrade.Common;
        public Tribe tribe = Tribe.Beast;

        [Header("2. 전투 기본 스탯")]
        public double baseDamage = 10.0;
        public float attackSpeed = 1.0f;
        public float attackRange = 1.0f;
        public AttackType attackType = AttackType.Single;
        public float splashRadius = 0f;

        [Header("3. 치명타 스탯")]
        [Range(0f, 1f)] public float criticalChance = 0.05f;
        public float criticalMultiplier = 1.5f;

        [Header("4. 마나 및 스킬 (고등급용)")]
        public float maxMana = 0f;
        public float manaGainOnAttack = 0f;
        public float manaRegenPerSec = 0f;
    }
}