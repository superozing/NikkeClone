using NikkeClone.Utils;
using UnityEngine;

/// <summary>
/// 버스트 스킬 발동 트리거를 감지하여 실행되는 버스트 스킬의 베이스 클래스입니다.
/// </summary>
public abstract class Burst_Base : SkillBase, ITriggerOnBurstUsed
{
    // ==================== Lifecycle ====================

    protected override void OnInitialize()
    {
        // 중앙 라우팅 방식을 사용하므로 별도의 구독 로직이 필요 없습니다.
    }

    public override void Dispose()
    {
        base.Dispose();
    }

    // ==================== Event Handlers ====================

    /// <summary>
    /// 버스트 이벤트가 발생했을 때 호출됩니다.
    /// 시전자가 자신이고, 버스트 단계가 이 스킬의 설정과 일치하면 실행합니다.
    /// </summary>
    public virtual void OnBurstUsed(int casterIdx, eBurstStage stage)
    {
        // 1. 시전자가 본인인지 체크
        if (casterIdx != _ownerIdx) return;

        // 2. 버스트 단계가 이 스킬이 담당하는 단계인지 체크 (Data의 burstStage 필드 활용)
        if (stage != (eBurstStage)_skillData.burstStage) return;

        Debug.Log($"<color=cyan>[Skill]</color> <b>{GetType().Name}</b> triggered by Burst Stage {stage}");

        // 실제 스킬 효과 실행
        ExecuteSkill();
    }

    /// <summary>
    /// 실제 스킬의 효과를 구현합니다.
    /// </summary>
    protected abstract void ExecuteSkill();

    public override void Tick(float deltaTime)
    {
        // 필요 시 오버라이드
    }
}
