using UnityEngine;

namespace TDGame.Field
{
    public class Tile : MonoBehaviour
    {
        public int gridX;
        public int gridY;
        public bool isOccupied = false;
        public GameObject currentUnit;

        /// <summary>
        /// 타일에 유닛을 배치합니다.
        /// </summary>
        public void PlaceUnit(GameObject unit)
        {
            currentUnit = unit;
            isOccupied = true;
        }

        /// <summary>
        /// 유닛이 다른 곳으로 이동하거나 합성/제거될 때 타일을 비웁니다.
        /// </summary>
        public void ClearTile()
        {
            currentUnit = null;
            isOccupied = false;
        }
    }
}