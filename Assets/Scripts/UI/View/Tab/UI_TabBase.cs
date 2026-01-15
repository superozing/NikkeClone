using UI;
using UnityEngine;

public abstract class UI_TabBase : UI_View
{
    public abstract eTabType TabType { get; }

    /// <summary>
    /// UI_TabGroupPopup에 의해 이 탭이 선택되었을 때(활성화될 때) 호출됩니다.
    /// View가 활성화될 때 데이터를 갱신하거나 ViewModel에 상태 변경을 알릴 필요가 있을 경우 오버라이딩 해야 해요.
    /// </summary>
    public virtual void OnTabSelected()
    {
        gameObject.SetActive(true);
        Debug.Log($"{TabType} 탭이 선택되었습니다.");
    }

    /// <summary>
    /// UI_TabGroupPopup에 의해 이 탭이 선택 해제되었을 때(비활성화될 때) 호출됩니다.
    /// View가 비활성화될 때 특정 로직이 필요할 경우 오버라이딩 해야 해요.
    /// </summary>
    public virtual void OnTabDeselected()
    {
        gameObject.SetActive(false);
        Debug.Log($"{TabType} 탭이 선택 해제되었습니다.");
    }
}
