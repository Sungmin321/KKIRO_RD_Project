using UnityEngine;

namespace TDGame.Field
{
    public class Tile : MonoBehaviour
    {
        public int gridX;
        public int gridY;
        public bool isOccupied = false;
        public GameObject placedUnit; // 드래그 핸들러가 참조할 유닛 오브젝트

        public void PlaceUnit(GameObject unit)
        {
            placedUnit = unit;
            isOccupied = true;
        }

        public void ClearTile()
        {
            placedUnit = null;
            isOccupied = false;
        }
    }
}