using UnityEngine;

namespace TDGame.Field
{
    public class Tile : MonoBehaviour
    {
        public int gridX;
        public int gridY;
        public bool isOccupied = false;
        public GameObject currentUnit = null;

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color defaultColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color highlightColor = new Color(0.4f, 0.8f, 0.4f, 0.8f);

        private void Awake()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            SetHighlight(false);
        }

        public void SetHighlight(bool active)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = active ? highlightColor : defaultColor;
        }

        public void PlaceUnit(GameObject unit)
        {
            currentUnit = unit;
            isOccupied = (unit != null);
            if (unit != null)
            {
                unit.transform.position = transform.position;
            }
        }

        public void ClearUnit()
        {
            currentUnit = null;
            isOccupied = false;
        }
    }
}