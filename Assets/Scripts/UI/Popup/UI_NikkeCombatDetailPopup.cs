using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Collections.Generic;
using UI;

/// <summary>
/// 니케 전투 상세정보 팝업 View 클래스입니다.
/// </summary>
public class UI_NikkeCombatDetailPopup : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_NikkeCombatDetailPopup";

    [SerializeField] private UI_NikkeCombatSlot _headerSlot;
    [SerializeField] private Button _btnClose;
    [SerializeField] private UI_EffectSlot[] _effectSlots; // Inspector에서 미리 캐싱된 슬롯들

    private NikkeCombatDetailPopupViewModel _viewModel;
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();
        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);

        if (_btnClose != null) _btnClose.onClick.AddListener(OnCloseClicked);
        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as NikkeCombatDetailPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // 헤더 슬롯에 SlotViewModel 바인딩
            if (_headerSlot != null)
            {
                _headerSlot.SetViewModel(_viewModel.SlotViewModel);
            }

            RefreshEffectList();
        }
    }

    private void RefreshEffectList()
    {
        if (_effectSlots == null) return;

        var slotVMs = _viewModel?.EffectSlotViewModels;
        int activeCount = slotVMs?.Length ?? 0;

        for (int i = 0; i < _effectSlots.Length; i++)
        {
            var slot = _effectSlots[i];
            if (slot == null) continue;

            if (i < activeCount)
            {
                slot.gameObject.SetActive(true);
                slot.SetViewModel(slotVMs[i]);
            }
            else
            {
                slot.gameObject.SetActive(false);
                // ViewModel 해제
                slot.SetViewModel(null);
            }
        }
    }

    private void OnCloseClicked() => _viewModel?.OnCloseClicked(this);
    private void OnEscapeAction(UnityEngine.InputSystem.InputAction.CallbackContext context) => OnCloseClicked();

    public async Task PlayShowAnimationAsync(float delay = 0) { if (_showAnim != null) await _showAnim.ExecuteAsync(delay); }
    public async Task PlayHideAnimationAsync(float delay = 0) { if (_hideAnim != null) await _hideAnim.ExecuteAsync(delay); }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Managers.Input.UnbindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }
}
