using UnityEngine;
using NikkeClone.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;

/// <summary>
/// 버스트 게이지와 하위 스킬 슬롯들을 총괄하는 뷰모델입니다.
/// </summary>
public class BurstGaugeViewModel : ViewModelBase
{
    public ReactiveProperty<float> Gauge { get; } = new(0f);
    public ReactiveProperty<eBurstStage> CurrentStage { get; } = new(eBurstStage.None);
    public ReactiveProperty<bool> IsFullBurst { get; } = new(false);

    public List<BurstSkillSlotViewModel> SlotViewModels { get; } = new();

    private CombatBurstSystem _burstSystem;
    private const int SQUAD_SIZE = 5;

    public BurstGaugeViewModel(CombatBurstSystem burstSystem)
    {
        _burstSystem = burstSystem;

        // 버스트 매니저와 데이터 바인딩
        if (_burstSystem != null)
        {
            _burstSystem.Gauge.OnValueChanged += (val) => Gauge.Value = val;
            _burstSystem.CurrentStage.OnValueChanged += (val) =>
            {
                CurrentStage.Value = val;
                UpdateSlots(val);
            };
            _burstSystem.IsFullBurst.OnValueChanged += (val) => IsFullBurst.Value = val;

            // 초기값 설정
            Gauge.Value = _burstSystem.Gauge.Value;
            CurrentStage.Value = _burstSystem.CurrentStage.Value;
            IsFullBurst.Value = _burstSystem.IsFullBurst.Value;
        }

        // 각 니케 슬롯 뷰모델 생성 (고정 5인 스쿼드)
        for (int i = 0; i < SQUAD_SIZE; i++)
        {
            var slotVM = new BurstSkillSlotViewModel(_burstSystem, i, RequestBurst);
            SlotViewModels.Add(slotVM);
        }

        UpdateSlots(CurrentStage.Value);
    }

    /// <summary>
    /// 하위 슬롯들의 리소스를 비동기로 초기화합니다.
    /// </summary>
    public async Task InitializeAsync()
    {
        foreach (var slot in SlotViewModels)
        {
            await slot.InitializeAsync();
        }
    }

    private void UpdateSlots(eBurstStage stage)
    {
        foreach (var slot in SlotViewModels)
        {
            slot.UpdateStatus(stage);
        }
    }

    /// <summary>
    /// UI에서 클릭 시 호출
    /// </summary>
    public void RequestBurst(int slotIndex)
    {
        _burstSystem?.RequestBurst(slotIndex);
    }
}
