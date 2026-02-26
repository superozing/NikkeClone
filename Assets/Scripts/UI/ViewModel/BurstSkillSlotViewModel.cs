using UnityEngine;
using NikkeClone.Utils;
using System.Threading.Tasks;
using UI;

/// <summary>
/// 개별 니케의 버스트 스킬 슬롯 정보를 담는 뷰모델입니다.
/// </summary>
public class BurstSkillSlotViewModel : ViewModelBase
{
    public ReactiveProperty<bool> IsAvailable { get; } = new(false);
    public ReactiveProperty<bool> IsVisible { get; } = new(false);
    public ReactiveProperty<float> CooldownRemaining { get; } = new(0f);
    public float CooldownTotal { get; private set; }

    public ReactiveProperty<Sprite> SkillIcon { get; } = new(null);
    public string SkillName { get; private set; }
    public string NikkeName { get; private set; }
    public int BurstStage { get; private set; }
    public int SlotIndex { get; private set; }

    private CombatBurstSystem _burstSystem;
    private System.Action<int> _onRequestBurst;

    public BurstSkillSlotViewModel(CombatBurstSystem burstSystem, int slotIndex, System.Action<int> onRequestBurst)
    {
        _burstSystem = burstSystem;
        SlotIndex = slotIndex;
        _onRequestBurst = onRequestBurst;

        if (burstSystem != null)
        {
            NikkeName = burstSystem.GetNikkeName(slotIndex);
            BurstStage = burstSystem.GetBurstLevel(slotIndex);

            var skillData = burstSystem.GetSkillData(slotIndex);
            if (skillData != null)
            {
                SkillName = skillData.name;
                CooldownTotal = burstSystem.GetCooldownTotal(slotIndex);
            }

            // 쿨타임 프로퍼티 바인딩 (실시간 갱신)
            var cdProperty = burstSystem.GetCooldownRemainingProperty(slotIndex);
            if (cdProperty != null)
            {
                cdProperty.OnValueChanged += (val) => CooldownRemaining.Value = val;
                CooldownRemaining.Value = cdProperty.Value;
            }
        }
    }

    /// <summary>
    /// 니케의 얼굴 아이콘 리소스를 비동기로 로드합니다.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (string.IsNullOrEmpty(NikkeName)) return;

        // 니케 얼굴 이미지 경로: Assets/Textures/Nikke/{Name}_Face
        string facePath = $"Assets/Textures/Nikke/{NikkeName}_Face";
        SkillIcon.Value = await Managers.Resource.LoadAsync<Sprite>(facePath);
    }

    /// <summary>
    /// UI 클릭 시 버스트 발동 요청
    /// </summary>
    /// Caller: UI_BurstSkillSlot.OnClick
    public void RequestBurst(int slotIndex)
    {
        _onRequestBurst?.Invoke(slotIndex);
    }

    public void UpdateStatus(eBurstStage currentStage)
    {
        if (_burstSystem == null || _burstSystem.IsNikkeDead(SlotIndex))
        {
            IsAvailable.Value = false;
            return;
        }

        // 1. 현재 버스트 단계가 니케의 버스트 레벨과 일치하는지
        bool stageMatch = (int)currentStage == BurstStage;

        // 2. 가시성 결정: 현재 단계와 일치할 때만 표시
        IsVisible.Value = stageMatch;

        // 3. 쿨타임 여부 (바인딩에 의해 실시간 갱신되므로 가용성 판단에만 사용)
        bool isCooldownFinished = CooldownRemaining.Value <= 0;

        IsAvailable.Value = stageMatch && isCooldownFinished;
    }
}
