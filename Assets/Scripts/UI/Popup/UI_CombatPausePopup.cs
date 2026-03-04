using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UI;

/// <summary>
/// 전투 일시정지 팝업 View 클래스입니다.
/// </summary>
public class UI_CombatPausePopup : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_CombatPausePopup";

    [SerializeField] private TMP_Text _txtElapsedTime;
    [SerializeField] private Button _btnResume;
    [SerializeField] private Button _btnRetry;
    [SerializeField] private Button _btnEndCombat;
    [SerializeField] private UI_NikkeCombatSlot[] _nikkeSlots;

    private CombatPausePopupViewModel _viewModel;
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();
        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);

        if (_btnResume != null) _btnResume.onClick.AddListener(OnResumeClicked);
        if (_btnRetry != null) _btnRetry.onClick.AddListener(OnRetryClicked);
        if (_btnEndCombat != null) _btnEndCombat.onClick.AddListener(OnEndCombatClicked);

        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CombatPausePopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // MVVM Bind Pattern 적용
            Bind(_viewModel.TimeText, time => { if (_txtElapsedTime != null) _txtElapsedTime.text = time; });

            for (int i = 0; i < _nikkeSlots.Length; i++)
            {
                if (_nikkeSlots[i] == null) continue;
                var slotVM = (i < _viewModel.SlotViewModels.Length) ? _viewModel.SlotViewModels[i] : null;
                _nikkeSlots[i].SetViewModel(slotVM);
            }
        }
    }

    private void OnResumeClicked() => _viewModel?.OnResumeClicked(this);
    private void OnRetryClicked() => _viewModel?.OnRetryClicked(this);
    private void OnEndCombatClicked() => _viewModel?.OnEndCombatClicked(this);
    private void OnEscapeAction(UnityEngine.InputSystem.InputAction.CallbackContext context) => OnResumeClicked();

    public async Task PlayShowAnimationAsync(float delay = 0) { if (_showAnim != null) await _showAnim.ExecuteAsync(delay); }
    public async Task PlayHideAnimationAsync(float delay = 0) { if (_hideAnim != null) await _hideAnim.ExecuteAsync(delay); }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Managers.Input.UnbindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }
}
