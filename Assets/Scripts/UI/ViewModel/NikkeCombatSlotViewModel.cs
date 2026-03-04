using System;
using System.Threading.Tasks;
using UnityEngine;
using UI;

/// <summary>
/// 일시정지 팝업 내 개별 니케의 전투 통계 스냅샷을 관리하는 ViewModel입니다.
/// </summary>
public class NikkeCombatSlotViewModel : ViewModelBase
{
    public ReactiveProperty<Sprite> ProfileImage { get; } = new();
    public ReactiveProperty<string> NikkeName { get; } = new();
    public ReactiveProperty<string> LevelText { get; } = new();
    public ReactiveProperty<long> DamageDealt { get; } = new();
    public ReactiveProperty<long> DamageTaken { get; } = new();
    public ReactiveProperty<long> HealReceived { get; } = new();
    public ReactiveProperty<bool> IsAlive { get; } = new();

    public ReactiveProperty<float> DamageDealtRatio { get; } = new();
    public ReactiveProperty<float> DamageTakenRatio { get; } = new();
    public ReactiveProperty<float> HealReceivedRatio { get; } = new();

    public Action OnClicked { get; }
    public int SlotIndex { get; }

    public NikkeCombatSlotViewModel(CombatNikke nikke, NikkeCombatRecord record, Action onClicked)
    {
        OnClicked = onClicked;

        if (nikke != null)
        {
            NikkeName.Value = nikke.NikkeName;
            LevelText.Value = $"Lv.{nikke.Level}";
            IsAlive.Value = !nikke.IsDead;
            SlotIndex = nikke.SlotIndex;

            // 이미지 로딩을 위한 이름 캡처 (CombatNikke 참조는 들고 있지 않음)
            if (nikke.GameData != null)
            {
                _ = LoadProfileImageAsync(nikke.GameData.name);
            }
        }

        if (record != null)
        {
            DamageDealt.Value = record.TotalDamageDealt;
            DamageTaken.Value = record.TotalDamageTaken;
            HealReceived.Value = record.TotalHealReceived;
        }
    }

    private async Task LoadProfileImageAsync(string nikkeDataName)
    {
        string cropPath = $"Assets/Textures/Nikke/{nikkeDataName}_Face";
        ProfileImage.Value = await Managers.Resource.LoadAsync<Sprite>(cropPath);
    }
}
