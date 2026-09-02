using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 신형 Input System 네임스페이스
using TMPro;
using TDGame.Combat;
using TDGame.Core;

namespace TDGame.UI
{
    public class UnitStatusUIController : MonoBehaviour
    {
        public static UnitStatusUIController Instance { get; private set; }

        [Header("UI 컴포넌트 연결")]
        [SerializeField] private GameObject panelParent; // UnitStatusPanel 오브젝트
        [SerializeField] private Image unitIconImage;
        [SerializeField] private TextMeshProUGUI unitNameText;
        [SerializeField] private TextMeshProUGUI unitInfoText;
        [SerializeField] private TextMeshProUGUI damageText;
        [SerializeField] private TextMeshProUGUI rangeAsText;

        private UnitCombat currentTargetUnit;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            HidePanel();
        }

        private void Update()
        {
            // 신형 Input System으로 마우스 우클릭 감지 -> 패널 닫기
            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame && panelParent != null && panelParent.activeSelf)
            {
                HidePanel();
            }
        }

        /// <summary>
        /// 특정 유닛의 정보를 패널에 표시
        /// </summary>
        public void ShowUnitStatus(UnitCombat unit)
        {
            if (unit == null || unit.unitData == null || panelParent == null) return;

            currentTargetUnit = unit;
            panelParent.SetActive(true);

            if (unitIconImage != null) unitIconImage.sprite = unit.unitData.unitSprite;
            if (unitNameText != null) unitNameText.text = unit.unitData.unitName;

            if (unitInfoText != null)
                unitInfoText.text = $"{unit.unitData.grade} / {unit.unitData.tribe}";

            UpdateStatText();
        }

        /// <summary>
        /// 강화 수치가 바뀌었을 때 실시간으로 텍스트 업데이트
        /// </summary>
        public void UpdateStatText()
        {
            if (currentTargetUnit == null || !panelParent.activeSelf) return;

            UnitDataSO data = currentTargetUnit.unitData;

            if (damageText != null)
            {
                damageText.text = $"공격력: <color=yellow>{currentTargetUnit.calculatedDamage:F0}</color> (기본 {data.baseDamage})";
            }

            if (rangeAsText != null)
            {
                rangeAsText.text = $"사거리: {data.attackRange} | 공속: {data.attackSpeed:F1}";
            }
        }

        public void HidePanel()
        {
            currentTargetUnit = null;
            if (panelParent != null) panelParent.SetActive(false);
        }
    }
}