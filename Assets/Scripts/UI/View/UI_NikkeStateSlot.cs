using UnityEngine;
using UnityEngine.UI;
using UI;

/// <summary>
/// 개별 니케의 상태를 간단하게 표시하는 UI 슬롯입니다.
/// </summary>
public class UI_NikkeStateSlot : UI_View
{
    [SerializeField] private Image _highlightImage; // Inspector 할당 (Phase 5)

    private NikkeStateViewModel _stateViewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        base.SetViewModel(viewModel);
        _stateViewModel = viewModel as NikkeStateViewModel;

        if (_stateViewModel == null)
        {
            if (viewModel != null)
                Debug.LogError($"[UI_NikkeStateSlot] Invalid ViewModel Type: {viewModel.GetType()}");
            return;
        }

        // Bind ReactiveProperties
        // 1. HP Ratio (TODO: Add Slider or Fill Image)
        Bind(_stateViewModel.HpRatio, ratio =>
        {
            // if (_hpSlider != null) _hpSlider.value = ratio;
        });

        // 2. IsDead (TODO: Add dead visual)
        Bind(_stateViewModel.IsDead, isDead =>
        {
            // if (_deadCover != null) _deadCover.SetActive(isDead);
        });

        // 3. Selection Highlight
        Bind(_stateViewModel.IsSelected, isSelected =>
        {
            if (_highlightImage != null)
                _highlightImage.gameObject.SetActive(isSelected);
        });
    }
}
