using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UI_CombatResultDefeatPopup : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_CombatResultDefeatPopup";

    [SerializeField] private Button _btnRetry;
    [SerializeField] private Button _btnUpgrade;
    [SerializeField] private Button _btnExit;
    [SerializeField] private CanvasGroup _canvasGroup;

    private CombatResultDefeatPopupViewModel _viewModel;
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();

        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);

        if (_btnRetry != null) _btnRetry.onClick.AddListener(() => _viewModel?.OnRetryClicked());
        if (_btnUpgrade != null) _btnUpgrade.onClick.AddListener(() => _viewModel?.OnUpgradeClicked());
        if (_btnExit != null) _btnExit.onClick.AddListener(() => _viewModel?.OnExitClicked());

        // ESC 키 바인딩 (Exit 로직과 동일하게 처리)
        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }

    private async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CombatResultDefeatPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_CombatResultDefeatPopup] 잘못된 ViewModel 타입이 주입되었습니다. Expected: {nameof(CombatResultDefeatPopupViewModel)}, Actual: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);
    }

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        if (_showAnim != null)
            await _showAnim.ExecuteAsync(delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0)
    {
        if (_hideAnim != null)
            await _hideAnim.ExecuteAsync(delay);
    }

    private void OnEscapeAction(UnityEngine.InputSystem.InputAction.CallbackContext context) => _viewModel?.OnExitClicked();

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Input.UnbindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);

        if (_btnRetry != null) _btnRetry.onClick.RemoveAllListeners();
        if (_btnUpgrade != null) _btnUpgrade.onClick.RemoveAllListeners();
        if (_btnExit != null) _btnExit.onClick.RemoveAllListeners();
    }
}
