using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 중인 모든 니케의 스킬 인스턴스를 소유하고 관리하는 시스템입니다.
/// </summary>
public class CombatSkillSystem
{
    private List<SkillBase> _activeSkills = new List<SkillBase>();

    /// <summary>
    /// 스쿼드 정보를 기반으로 각 니케가 보유한 스킬들을 팩토리로부터 생성하고 초기화합니다.
    /// </summary>
    /// <param name="combatSystem">전투 시스템 코어</param>
    /// <param name="trigger">이벤트 브로드캐스터</param>
    /// <param name="squadData">전투에 참여한 니케들의 게임 데이터 배열</param>
    public void LoadNikkeSkills(CombatSystem combatSystem, CombatTriggerSystem trigger, NikkeGameData[] squadData)
    {
        for (int i = 0; i < squadData.Length; i++)
        {
            var data = squadData[i];
            if (data == null || data.skills == null) continue;

            foreach (var skillData in data.skills)
            {
                // 패시브 타입인 것만 로딩 (버스트는 BurstSystem에서 별도 관리하거나 통합 가능)
                if (skillData.skillTypeName != "Passive") continue;

                SkillBase newSkill = SkillFactory.CreateSkill(skillData.skillID);
                if (newSkill != null)
                {
                    // CombatSystem, TriggerSystem, Owner Index 주입
                    newSkill.Initialize(combatSystem, trigger, i, skillData);
                    _activeSkills.Add(newSkill);
                }
            }
        }
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
    /// 시스템 종료 시 모든 스킬의 구독을 해제하고 리스트를 비웁니다.
    /// </summary>
    public void Cleanup()
    {
        for (int i = 0; i < _activeSkills.Count; i++)
        {
            _activeSkills[i].Dispose();
        }
        _activeSkills.Clear();
    }
}
