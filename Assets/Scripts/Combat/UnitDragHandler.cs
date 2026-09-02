using UnityEngine;
using UnityEngine.InputSystem;
using TDGame.Core;
using TDGame.Field;
using TDGame.UI;

namespace TDGame.Combat
{
    public class UnitDragHandler : MonoBehaviour
    {
        public Tile currentTile;
        private Vector3 screenPoint;
        private Vector3 offset;
        private Vector3 originalPosition;
        private bool isDragging = false;
        private UnitCombat unitCombat;
        private Collider2D unitCollider;

        private void Start()
        {
            unitCombat = GetComponent<UnitCombat>();
            unitCollider = GetComponent<Collider2D>();
        }

        private Vector2 GetCurrentMousePosition()
        {
            return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;
            if (Camera.main == null) return;

            // 유닛 클릭 시 하단 상태창 UI 갱신
            if (UnitStatusUIController.Instance != null && unitCombat != null)
            {
                UnitStatusUIController.Instance.ShowUnitStatus(unitCombat);
            }

            isDragging = true;
            originalPosition = transform.position;

            // 드래그 중 바닥 타일 레이캐스트를 가리지 않도록 콜라이더 비활성화
            if (unitCollider != null) unitCollider.enabled = false;

            Vector2 mousePos = GetCurrentMousePosition();
            screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, screenPoint.z));

            if (currentTile != null)
            {
                currentTile.ClearTile();
            }
        }

        private void OnMouseDrag()
        {
            if (!isDragging || Camera.main == null) return;

            Vector2 mousePos = GetCurrentMousePosition();
            Vector3 curScreenPoint = new Vector3(mousePos.x, mousePos.y, screenPoint.z);
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;

            curPosition.z = -0.5f; // 타일보다 앞에 보이도록 설정
            transform.position = curPosition;
        }

        private void OnMouseUp()
        {
            if (!isDragging || Camera.main == null) return;
            isDragging = false;

            Vector2 mousePos = GetCurrentMousePosition();
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, -Camera.main.transform.position.z));
            Vector2 rayOrigin = new Vector2(worldPoint.x, worldPoint.y);

            // Tile 레이어 마스크를 가진 타일 탐색
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero, 0f, LayerMask.GetMask("Tile"));

            if (hit.collider != null)
            {
                Tile targetTile = hit.collider.GetComponent<Tile>();

                // 1. 빈 타일인 경우 배치
                if (targetTile != null && !targetTile.isOccupied)
                {
                    PlaceOnTile(targetTile);
                    return;
                }
                // 2. 다른 유닛이 점유 중인 타일인 경우 위치 맞바꾸기(Swap)
                else if (targetTile != null && targetTile.isOccupied && targetTile != currentTile)
                {
                    GameObject otherUnit = targetTile.placedUnit;
                    Tile otherOriginalTile = targetTile;

                    if (currentTile != null && otherUnit != null)
                    {
                        UnitDragHandler otherDrag = otherUnit.GetComponent<UnitDragHandler>();
                        if (otherDrag != null)
                        {
                            currentTile.PlaceUnit(otherUnit);
                            otherDrag.currentTile = currentTile;
                        }
                    }

                    PlaceOnTile(otherOriginalTile);
                    return;
                }
            }

            // 배치할 타일을 찾지 못했을 때 기존 타일로 복귀
            if (currentTile != null)
            {
                PlaceOnTile(currentTile);
            }
            else
            {
                transform.position = originalPosition;
                if (unitCollider != null) unitCollider.enabled = true;
            }
        }

        private void PlaceOnTile(Tile tile)
        {
            currentTile = tile;
            Vector3 pos = currentTile.transform.position;
            pos.z = -0.1f;
            transform.position = pos;
            currentTile.PlaceUnit(gameObject);

            // 배치가 끝나면 다시 클릭할 수 있도록 콜라이더 활성화
            if (unitCollider != null) unitCollider.enabled = true;
        }
    }
}