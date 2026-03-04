using System.Collections.Generic;
using UnityEngine;
using UI;

/// <summary>
/// 개별 니케의 상세 전투 정보를 관리하는 ViewModel입니다.
/// </summary>
public class NikkeCombatDetailPopupViewModel : ViewModelBase
{
    public NikkeCombatSlotViewModel SlotViewModel { get; }
    public EffectSlotViewModel[] EffectSlotViewModels { get; }

    public NikkeCombatDetailPopupViewModel(NikkeCombatSlotViewModel slotVM, CombatNikke nikke)
    {
        SlotViewModel = slotVM;

        if (nikke != null && nikke.Status != null)
        {
            var effects = nikke.Status.GetActiveEffects();
            EffectSlotViewModels = new EffectSlotViewModel[effects.Count];
            for (int i = 0; i < effects.Count; i++)
            {
                EffectSlotViewModels[i] = new EffectSlotViewModel(effects[i]);
                EffectSlotViewModels[i].AddRef();
            }
        }
        else
        {
            EffectSlotViewModels = new EffectSlotViewModel[0];
        }
    }

    public void OnCloseClicked(UI_Popup popup)
    {
        Managers.UI.Close(popup);
    }

    protected override void OnDispose()
    {
        if (EffectSlotViewModels != null)
        {
            foreach (var vm in EffectSlotViewModels)
            {
                vm?.Release();
            }
        }
        base.OnDispose();
    }
}
