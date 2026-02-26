using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// 개별 니케의 버스트 스킬 슬롯을 표시하고 클릭 입력을 처리합니다.
/// </summary>
public class UI_BurstSkillSlot : UI_View
{
    [SerializeField] private Image _skillIcon;
    [SerializeField] private Image _cooldownOverlay;
    [SerializeField] private TMP_Text _cooldownText;
    [SerializeField] private Button _button;
    [SerializeField] private GameObject _availableEffect;

    private BurstSkillSlotViewModel _burstSlotViewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _button.onClick.RemoveAllListeners();

        _burstSlotViewModel = viewModel as BurstSkillSlotViewModel;
        base.SetViewModel(viewModel);

        if (_burstSlotViewModel == null) return;

        // 초기 설정
        if (_availableEffect != null)
            _availableEffect.SetActive(false);

        // 가용성 상태 바인딩
        Bind(_burstSlotViewModel.IsAvailable, UpdateAvailability);

        // 가시성 상태 바인딩
        Bind(_burstSlotViewModel.IsVisible, UpdateVisibility);

        // 스킬 아이콘(니케 얼굴) 바인딩
        Bind(_burstSlotViewModel.SkillIcon, sprite =>
        {
            if (_skillIcon != null) _skillIcon.sprite = sprite;
        });

        // 쿨타임 바인딩
        Bind(_burstSlotViewModel.CooldownRemaining, UpdateCooldown);

        // 클릭 이벤트
        _button.onClick.AddListener(() => _burstSlotViewModel.RequestBurst(_burstSlotViewModel.SlotIndex));
    }

    private void UpdateAvailability(bool isAvailable)
    {
        if (_availableEffect != null)
            _availableEffect.SetActive(isAvailable);

        // 연출: 발동 가능 시 글로우 효과 등 추가 가능
    }

    private void UpdateVisibility(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    private void UpdateCooldown(float remaining)
    {
        if (remaining > 0)
        {
            if (_cooldownOverlay != null)
            {
                _cooldownOverlay.gameObject.SetActive(true);
                _cooldownOverlay.fillAmount = remaining / _burstSlotViewModel.CooldownTotal;
            }
            if (_cooldownText != null)
                _cooldownText.text = Mathf.CeilToInt(remaining).ToString();
        }
        else
        {
            if (_cooldownOverlay != null)
                _cooldownOverlay.gameObject.SetActive(false);
            if (_cooldownText != null)
                _cooldownText.text = "";
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _burstSlotViewModel = null;
    }
}
