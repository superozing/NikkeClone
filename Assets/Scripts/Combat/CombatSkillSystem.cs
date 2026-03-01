using System.Collections.Generic;
using NikkeClone.Utils;

/// <summary>
/// 전투 중인 모든 니케의 스킬 인스턴스를 소유하고 관리하는 시스템입니다.
/// 트리거 인터페이스를 기반으로 스킬들을 분류하고 이벤트를 라우팅합니다.
/// </summary>
public class CombatSkillSystem
{
    private List<SkillBase> _activeSkills = new List<SkillBase>();

    // ==================== Trigger Handlers (Phase 10) ====================
    private List<ITriggerOnAllyHit> _onAllyHitHandlers = new List<ITriggerOnAllyHit>();
    private List<ITriggerOnEnemyDied> _onEnemyDiedHandlers = new List<ITriggerOnEnemyDied>();
    private List<ITriggerOnBurstUsed> _onBurstUsedHandlers = new List<ITriggerOnBurstUsed>();

    private CombatTriggerSystem _triggerSystem;

    /// <summary>
    /// 스쿼드 정보를 기반으로 각 니케가 보유한 스킬들을 팩토리로부터 생성하고 초기화합니다.
    /// 생성된 스킬은 인터페이스 기반으로 라우팅 그룹에 할당됩니다.
    /// </summary>
    public void LoadNikkeSkills(CombatSystem combatSystem, CombatTriggerSystem trigger, NikkeGameData[] squadData)
    {
        _triggerSystem = trigger;

        for (int i = 0; i < squadData.Length; i++)
        {
            var data = squadData[i];
            if (data == null || data.skills == null) continue;

            foreach (var skillData in data.skills)
            {
                SkillBase newSkill = SkillFactory.CreateSkill(skillData.skillID);
                if (newSkill != null)
                {
                    // 1. 초기화 (TriggerSystem 주입 제거됨)
                    newSkill.Initialize(combatSystem, i, skillData);
                    _activeSkills.Add(newSkill);

                    // 2. 인터페이스 기반 라우팅 그룹 등록
                    if (newSkill is ITriggerOnAllyHit onHit) _onAllyHitHandlers.Add(onHit);
                    if (newSkill is ITriggerOnEnemyDied onDied) _onEnemyDiedHandlers.Add(onDied);
                    if (newSkill is ITriggerOnBurstUsed onBurst) _onBurstUsedHandlers.Add(onBurst);
                }
            }
        }

        // 3. 트리거 시스템에 중앙 라우팅 메서드 바인딩
        if (_triggerSystem != null)
        {
            _triggerSystem.OnAllyHitEnemy += HandleAllyHit_Route;
            _triggerSystem.OnEnemyDied += HandleEnemyDied_Route;
            _triggerSystem.OnBurstSkillUsed += HandleBurstUsed_Route;
        }
    }

    // ==================== Routing Entry Points ====================

    private void HandleAllyHit_Route(int attackerIdx)
    {
        for (int i = 0; i < _onAllyHitHandlers.Count; i++)
            _onAllyHitHandlers[i].OnAllyHitEnemy(attackerIdx);
    }

    private void HandleEnemyDied_Route(CombatRapture rapture)
    {
        for (int i = 0; i < _onEnemyDiedHandlers.Count; i++)
            _onEnemyDiedHandlers[i].OnEnemyDied(rapture);
    }

    private void HandleBurstUsed_Route(int casterIdx, eBurstStage stage)
    {
        for (int i = 0; i < _onBurstUsedHandlers.Count; i++)
            _onBurstUsedHandlers[i].OnBurstUsed(casterIdx, stage);
    }

    /// <summary>
    /// 모든 활성화된 스킬의 타이머/쿨타임을 갱신합니다.
    /// </summary>
    public void Tick(float deltaTime)
    {
        for (int i = 0; i < _activeSkills.Count; i++)
        {
            _activeSkills[i].Tick(deltaTime);
        }
    }

    /// <summary>
    /// 시스템 종료 시 모든 스킬을 정리하고 구독을 해제합니다.
    /// </summary>
    public void Cleanup()
    {
        // 트리거 시스템 바인딩 해제
        if (_triggerSystem != null)
        {
            _triggerSystem.OnAllyHitEnemy -= HandleAllyHit_Route;
            _triggerSystem.OnEnemyDied -= HandleEnemyDied_Route;
            _triggerSystem.OnBurstSkillUsed -= HandleBurstUsed_Route;
        }

        for (int i = 0; i < _activeSkills.Count; i++)
        {
            _activeSkills[i].Dispose();
        }

        _activeSkills.Clear();
        _onAllyHitHandlers.Clear();
        _onEnemyDiedHandlers.Clear();
        _onBurstUsedHandlers.Clear();
    }
}
