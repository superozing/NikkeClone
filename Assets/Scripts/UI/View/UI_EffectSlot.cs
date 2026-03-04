using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// 상세 팝업 내 개별 효과들을 표시하는 슬롯 클래스입니다.
/// </summary>
public class UI_EffectSlot : UI_View
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _txtName;
    [SerializeField] private TMP_Text _txtDesc;
    [SerializeField] private TMP_Text _txtTime;

    private EffectSlotViewModel _slotViewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _slotViewModel = viewModel as EffectSlotViewModel;
        base.SetViewModel(viewModel);

        if (_slotViewModel != null)
        {
            // MVVM Bind Pattern 적용
            Bind(_slotViewModel.EffectIcon, sprite => { if (_icon != null) _icon.sprite = sprite; });
            Bind(_slotViewModel.EffectName, name => { if (_txtName != null) _txtName.text = name; });
            Bind(_slotViewModel.EffectDesc, desc => { if (_txtDesc != null) _txtDesc.text = desc; });
            Bind(_slotViewModel.EffectTime, time => { if (_txtTime != null) _txtTime.text = time; });
        }
    }
}
