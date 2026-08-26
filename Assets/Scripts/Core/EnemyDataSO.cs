using UnityEngine;

namespace TDGame.Core
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TDGame/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("몬스터 기본 스탯")]
        public string enemyName = "Slime";
        public float moveSpeed = 1.5f;
        public float baseArmor = 0f; // 방어력 (물리/마법 공통 적용)
    }
}