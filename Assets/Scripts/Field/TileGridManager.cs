using System.Collections.Generic;
using UnityEngine;
using TDGame.Core;
using TDGame.Combat; // <-- 이 줄을 추가합니다!

namespace TDGame.Field
{
    public class TileGridManager : MonoBehaviour
    {
        public static TileGridManager Instance { get; private set; }

        [Header("그리드 설정 (운빨존많겜 기준 3 x 6)")]
        public int rows = 3;
        public int columns = 6;
        public float tileSize = 1.2f;
        public float tileSpacing = 0.1f;

        [Header("프리팹 연결")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject unitBasePrefab; // UnitCombat이 붙은 기본 유닛 프리팹

        [Header("1단계(일반) 소환 유닛 데이터 풀")]
        [SerializeField] private List<UnitDataSO> commonUnitPool = new List<UnitDataSO>();

        [Header("소환 비용 설정")]
        public double baseSummonCost = 10.0;
        public double summonCostIncrease = 2.0; // 소환할 때마다 증가하는 골드
        private int totalSummonCount = 0;

        private List<Tile> allTiles = new List<Tile>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            GenerateGrid();
        }

        private void GenerateGrid()
        {
            allTiles.Clear();
            float startX = -((columns - 1) * (tileSize + tileSpacing)) / 2f;
            float startY = -((rows - 1) * (tileSize + tileSpacing)) / 2f;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Vector3 pos = new Vector3(startX + c * (tileSize + tileSpacing), startY + r * (tileSize + tileSpacing), 0f);
                    GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tileObj.name = $"Tile_{r}_{c}";

                    Tile tileComp = tileObj.GetComponent<Tile>();
                    tileComp.gridX = c;
                    tileComp.gridY = r;
                    allTiles.Add(tileComp);
                }
            }
        }

        public double GetCurrentSummonCost()
        {
            return baseSummonCost + (totalSummonCount * summonCostIncrease);
        }

        /// <summary>
        /// UI 소환 버튼에 연결할 함수
        /// </summary>
        public bool TrySummonUnit()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[에러] 씬에 @GameManager 오브젝트가 없습니다!");
                return false;
            }

            if (commonUnitPool == null || commonUnitPool.Count == 0)
            {
                Debug.LogError("[에러] @GridManager의 Common Unit Pool이 비어있습니다. UnitDataSO를 등록해주세요!");
                return false;
            }

            if (unitBasePrefab == null)
            {
                Debug.LogError("[에러] @GridManager의 Unit Base Prefab 슬롯이 비어있습니다!");
                return false;
            }

            double cost = GetCurrentSummonCost();

            if (GameManager.Instance.currentGold < cost)
            {
                Debug.LogWarning("골드가 부족합니다!");
                return false;
            }

            List<Tile> emptyTiles = allTiles.FindAll(t => !t.isOccupied);
            if (emptyTiles.Count == 0)
            {
                Debug.LogWarning("필드에 빈 공간이 없습니다!");
                return false;
            }

            GameManager.Instance.currentGold -= cost;
            totalSummonCount++;

            Tile targetTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
            UnitDataSO randomUnitData = commonUnitPool[Random.Range(0, commonUnitPool.Count)];

            GameObject newUnit = Instantiate(unitBasePrefab, targetTile.transform.position, Quaternion.identity);
            UnitCombat combatComp = newUnit.GetComponent<UnitCombat>();
            if (combatComp != null)
            {
                combatComp.unitData = randomUnitData;
                combatComp.RecalculateStats();
            }

            targetTile.PlaceUnit(newUnit);
            Debug.Log($"<color=cyan>[소환 성공]</color> {randomUnitData.unitName} 소환 완료 (소모 골드: {cost})");
            return true;
        }
    }
}