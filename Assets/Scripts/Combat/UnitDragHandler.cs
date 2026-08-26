using UnityEngine;
using UnityEngine.EventSystems;
using TDGame.Field;

namespace TDGame.Combat
{
    [RequireComponent(typeof(Collider2D))]
    public class UnitDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Vector3 startPosition;
        public Tile currentTile;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
            FindInitialTile();
        }

        public void FindInitialTile()
        {
            Collider2D col = Physics2D.OverlapPoint(transform.position);
            if (col != null)
            {
                Tile tile = col.GetComponent<Tile>();
                if (tile != null)
                {
                    currentTile = tile;
                    tile.PlaceUnit(gameObject);
                }
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            startPosition = transform.position;
            // 드래그 중인 유닛을 화면 맨 앞으로 표시
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 20;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0f;
            transform.position = mousePos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sortingOrder = 5;

            // 마우스 위치에 있는 타일 탐색
            Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
            Tile targetTile = null;

            foreach (var hit in hits)
            {
                Tile t = hit.GetComponent<Tile>();
                if (t != null)
                {
                    targetTile = t;
                    break;
                }
            }

            // 1. 빈 타일로 이동
            if (targetTile != null && !targetTile.isOccupied)
            {
                if (currentTile != null) currentTile.ClearTile();
                targetTile.PlaceUnit(gameObject);
                currentTile = targetTile;
                transform.position = targetTile.transform.position;
            }
            // 2. 다른 유닛과 위치 맞교환 (Swap)
            else if (targetTile != null && targetTile.isOccupied && targetTile != currentTile)
            {
                GameObject otherUnit = targetTile.currentUnit;

                if (currentTile != null)
                {
                    currentTile.currentUnit = otherUnit;
                    otherUnit.transform.position = currentTile.transform.position;
                    UnitDragHandler otherHandler = otherUnit.GetComponent<UnitDragHandler>();
                    if (otherHandler != null) otherHandler.currentTile = currentTile;
                }

                targetTile.currentUnit = gameObject;
                currentTile = targetTile;
                transform.position = targetTile.transform.position;
            }
            // 3. 타일 바깥에 떨어뜨린 경우 원위치 복귀
            else
            {
                transform.position = startPosition;
            }
        }
    }
}