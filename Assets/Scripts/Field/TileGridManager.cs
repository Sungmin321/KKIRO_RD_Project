using System.Collections.Generic;
using UnityEngine;
using TDGame.Core;
using TDGame.Combat;

namespace TDGame.Field
{
    public class TileGridManager : MonoBehaviour
    {
        public static TileGridManager Instance { get; private set; }

        [Header("그리드 규격 (4행 6열)")]
        public const int ROWS = 4;
        public const int COLS = 6;
        public const float TILE_SPACING = 1.3f;

        [Header("프리팹 및 풀")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private GameObject unitBasePrefab;
        [SerializeField] private List<UnitDataSO> commonUnitPool;

        private Tile[,] gridTiles = new Tile[ROWS, COLS];

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
            float startX = -((COLS - 1) * TILE_SPACING) / 2f;
            float startY = -((ROWS - 1) * TILE_SPACING) / 2f;

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    Vector3 pos = new Vector3(startX + (c * TILE_SPACING), startY + (r * TILE_SPACING), 0f);
                    GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tileObj.name = $"Tile_{r}_{c}";

                    Tile tile = tileObj.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tile.gridX = c;
                        tile.gridY = r;
                        gridTiles[r, c] = tile;
                    }
                }
            }
        }

        public double GetCurrentSummonCost()
        {
            return (GameManager.Instance != null) ? GameManager.Instance.GetSummonCost() : 10.0;
        }

        public bool TrySummonUnit()
        {
            double cost = GetCurrentSummonCost();

            if (GameManager.Instance == null || GameManager.Instance.currentGold < cost)
            {
                Debug.LogWarning("[소환 실패] 골드가 부족합니다.");
                return false;
            }

            List<Tile> emptyTiles = new List<Tile>();
            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    if (gridTiles[r, c] != null && !gridTiles[r, c].isOccupied)
                    {
                        emptyTiles.Add(gridTiles[r, c]);
                    }
                }
            }

            if (emptyTiles.Count == 0)
            {
                Debug.LogWarning("[소환 실패] 타일에 빈 공간이 없습니다.");
                return false;
            }

            GameManager.Instance.currentGold -= cost;
            GameManager.Instance.summonCount++;

            Tile targetTile = emptyTiles[Random.Range(0, emptyTiles.Count)];
            UnitDataSO randomData = commonUnitPool[Random.Range(0, commonUnitPool.Count)];

            Vector3 spawnPos = targetTile.transform.position;
            spawnPos.z = -0.1f;

            GameObject newUnit = Instantiate(unitBasePrefab, spawnPos, Quaternion.identity);

            // 유닛별 외형 스프라이트 교체
            SpriteRenderer sr = newUnit.GetComponent<SpriteRenderer>();
            if (sr != null && randomData.unitSprite != null)
            {
                sr.sprite = randomData.unitSprite;
            }

            UnitCombat combat = newUnit.GetComponent<UnitCombat>();
            if (combat != null) combat.unitData = randomData;

            UnitDragHandler dragHandler = newUnit.GetComponent<UnitDragHandler>();
            if (dragHandler != null) dragHandler.currentTile = targetTile;

            targetTile.PlaceUnit(newUnit);
            return true;
        }
    }
}