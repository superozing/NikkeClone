using UnityEngine;
using TMPro;

/// <summary>
/// 개별 니케의 상태를 간단하게 표시하는 UI 슬롯입니다.
/// </summary>
public class UI_NikkeStateSlot : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Image _highlightImage; // Inspector 할당 (Phase 5)

    /// <summary>
    /// 슬롯의 하이라이트 상태를 설정합니다. (조작 중인지 여부)
    /// Caller: UI_CombatHUD.SetActiveNikkeSlot()
    /// </summary>
    public void SetControlled(bool isControlled)
    {
        if (_highlightImage != null)
        {
            _highlightImage.gameObject.SetActive(isControlled);
        }
    }
}
