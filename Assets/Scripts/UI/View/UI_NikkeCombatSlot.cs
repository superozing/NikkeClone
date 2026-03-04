using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// 일시정지 팝업 내 개별 니케 전투 정보 슬롯 View 클래스입니다.
/// </summary>
public class UI_NikkeCombatSlot : UI_View
{
    [SerializeField] private Image _faceImage;
    [SerializeField] private TMP_Text _txtName;
    [SerializeField] private TMP_Text _txtLevel;

    [SerializeField] private Image _fillDamageDealt;
    [SerializeField] private TMP_Text _txtDamageDealt;

    [SerializeField] private Image _fillDamageTaken;
    [SerializeField] private TMP_Text _txtDamageTaken;

    [SerializeField] private Image _fillHealReceived;
    [SerializeField] private TMP_Text _txtHealReceived;

    [SerializeField] private Button _btnSlot;
    [SerializeField] private CanvasGroup _deadOverlay;

    private NikkeCombatSlotViewModel _slotViewModel;

    protected override void Awake()
    {
        base.Awake();
        if (_btnSlot != null) _btnSlot.onClick.AddListener(OnSlotClicked);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _slotViewModel = viewModel as NikkeCombatSlotViewModel;
        base.SetViewModel(viewModel);

        if (_slotViewModel != null)
        {
            // MVVM Bind Pattern 적용
            Bind(_slotViewModel.NikkeName, name => { if (_txtName != null) _txtName.text = name; });
            Bind(_slotViewModel.LevelText, level => { if (_txtLevel != null) _txtLevel.text = level; });
            Bind(_slotViewModel.IsAlive, alive =>
            {
                if (_deadOverlay != null) _deadOverlay.alpha = alive ? 0f : 1f;
                if (_btnSlot != null) _btnSlot.interactable = alive;
            });

            Bind(_slotViewModel.DamageDealt, val => { if (_txtDamageDealt != null) _txtDamageDealt.text = val.ToString("N0"); });
            Bind(_slotViewModel.DamageDealtRatio, ratio => { if (_fillDamageDealt != null) _fillDamageDealt.fillAmount = ratio; });

            Bind(_slotViewModel.DamageTaken, val => { if (_txtDamageTaken != null) _txtDamageTaken.text = val.ToString("N0"); });
            Bind(_slotViewModel.DamageTakenRatio, ratio => { if (_fillDamageTaken != null) _fillDamageTaken.fillAmount = ratio; });

            Bind(_slotViewModel.HealReceived, val => { if (_txtHealReceived != null) _txtHealReceived.text = val.ToString("N0"); });
            Bind(_slotViewModel.HealReceivedRatio, ratio => { if (_fillHealReceived != null) _fillHealReceived.fillAmount = ratio; });

            Bind(_slotViewModel.ProfileImage, sprite => { if (_faceImage != null) _faceImage.sprite = sprite; });
        }
    }

    private void OnSlotClicked()
    {
        // 부모 View를 찾지 않고, 자신의 ViewModel에서 제공하는 콜백 호출
        _slotViewModel?.OnClicked?.Invoke();
    }
}
