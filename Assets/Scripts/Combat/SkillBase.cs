using UnityEngine;

/// <summary>
/// 모든 스킬(패시브/버스트)의 추상 기반 클래스입니다.
/// 인스턴스화 될 때 주체(Owner)와 컨텍스트를 주입받아 독립적으로 작동합니다.
/// </summary>
public abstract class SkillBase
{
    protected CombatSystem _combatSystem;
    protected CombatTriggerSystem _triggerSystem;

    protected int _ownerIdx; // 이 스킬을 소유한 니케의 슬롯 인덱스 (0~4)
    protected SkillData _skillData;

    /// <summary>
    /// 스킬 인스턴스를 초기화하고 필요한 트리거를 구독합니다.
    /// </summary>
    public void Initialize(CombatSystem combatSystem, CombatTriggerSystem triggerSystem, int ownerIdx, SkillData data)
    {
        _combatSystem = combatSystem;
        _triggerSystem = triggerSystem;
        _ownerIdx = ownerIdx;
        _skillData = data;

        OnInitialize();
    }

    /// <summary>
    /// 지속/틱 효과 타이머 업데이트용 (CombatSkillSystem에서 매 프레임 호출)
    /// </summary>
    public virtual void Tick(float deltaTime)
    {
        // 쿨타임 로직 삭제, 지속 시간/틱 효과 처리용으로 사용 가능
    }

    /// <summary>
    /// 파생 클래스에서 스킬별 구독 로직을 구현합니다.
    /// </summary>
    protected abstract void OnInitialize();

    /// <summary>
    /// 정리 시 구독 해제 등을 수행합니다.
    /// </summary>
    public virtual void Dispose() { }
}
