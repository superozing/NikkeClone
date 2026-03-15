using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UI;

public class UI_CombatResultVictoryPopup : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_CombatResultVictoryPopup";

    [SerializeField] private UI_Icon[] _rewardItemSlots;
    [SerializeField] private Button _btnBackground;

    [SerializeField] private TextMeshProUGUI _txtStageInfo;

    private CombatResultVictoryPopupViewModel _viewModel;
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();

        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);

        if (_btnBackground != null)
        {
            _btnBackground.onClick.AddListener(OnBackgroundClicked);
        }

        Managers.Input.BindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);
    }

    private async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CombatResultVictoryPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_CombatResultVictoryPopup] 잘못된 ViewModel 타입이 주입되었습니다. Expected: {nameof(CombatResultVictoryPopupViewModel)}, Actual: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            Bind(_viewModel.RewardItemViewModels, OnRewardListChanged);
            Bind(_viewModel.StageInfo, info => { if (_txtStageInfo != null) _txtStageInfo.text = info; });
        }
    }

    private void OnRewardListChanged(List<StageRewardItemIconViewModel> rewards)
    {
        UpdateRewardSlots(rewards);
    }

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        if (_showAnim != null)
            await _showAnim.ExecuteAsync(delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0)
    {
        if (_hideAnim != null)
            await _hideAnim.ExecuteAsync();
    }

    private void UpdateRewardSlots(List<StageRewardItemIconViewModel> rewards)
    {
        if (_rewardItemSlots == null)
        {
            Debug.LogError("[UI_CombatResultVictoryPopup] _rewardItemSlots is not bound in the Inspector!");
            return;
        }

        for (int i = 0; i < _rewardItemSlots.Length; i++)
        {
            if (i < rewards.Count)
            {
                _rewardItemSlots[i].gameObject.SetActive(true);
                _rewardItemSlots[i].SetViewModel(rewards[i]);
            }
            else
            {
                _rewardItemSlots[i].gameObject.SetActive(false);
                _rewardItemSlots[i].SetViewModel(null);
            }
        }
    }

    private void OnBackgroundClicked()
    {
        _viewModel?.OnScreenClicked();
    }

    private void OnEscapeAction(UnityEngine.InputSystem.InputAction.CallbackContext context) => OnBackgroundClicked();

    protected override void OnDestroy()
    {
        base.OnDestroy();

        Managers.Input.UnbindAction("Close", OnEscapeAction, UnityEngine.InputSystem.InputActionPhase.Performed);

        if (_btnBackground != null)
        {
            _btnBackground.onClick.RemoveListener(OnBackgroundClicked);
        }
    }
}
