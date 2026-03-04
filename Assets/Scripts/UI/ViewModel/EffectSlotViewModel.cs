using UnityEngine;
using UI;

/// <summary>
/// 개별 효과 슬롯의 데이터를 관리하는 ViewModel입니다.
/// </summary>
public class EffectSlotViewModel : ViewModelBase
{
    public ReactiveProperty<Sprite> EffectIcon { get; } = new();
    public ReactiveProperty<string> EffectName { get; } = new();
    public ReactiveProperty<string> EffectDesc { get; } = new();
    public ReactiveProperty<string> EffectTime { get; } = new();

    public EffectSlotViewModel(ActiveEffect effect)
    {
        if (effect == null) return;

        EffectIcon.Value = effect.Data.Icon;
        EffectName.Value = effect.Data.EffectName;
        EffectDesc.Value = effect.Data.Description;
        EffectTime.Value = effect.Data.Duration <= 0f ? "PERMANENT" : $"{effect.RemainingTime:F1}s";
    }
}
