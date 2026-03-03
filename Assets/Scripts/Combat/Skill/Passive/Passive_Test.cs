using UnityEngine;

/// <summary>
/// 테스트용 패시브 스킬: 아군(Ally)이 적을 3회 처치할 때마다 발동합니다.
/// </summary>
public class Passive_Test : SkillBase, ITriggerOnEnemyDied
{
    private int _targetKillCount = 3;
    private int _currentKills = 0;

    protected override void OnInitialize()
    {
        // 중앙 라우팅 방식을 사용하므로 별도의 구독 로직이 필요 없습니다.
    }

    public void OnEnemyDied(CombatRapture deadRapture)
    {
        // 글로벌 킬 (누가 죽였는지는 상관없이 적이 죽으면 카운트)
        _currentKills++;

        // 디버그용 로그
        Debug.Log($"<color=yellow>[PassiveSkill]</color> Global Kill Count: {_currentKills}/{_targetKillCount}");

        if (_currentKills >= _targetKillCount)
        {
            _currentKills = 0;

            ExecuteSkill();
        }
    }

    private void ExecuteSkill()
    {
        // 실제 스킬 동작: CombatSystem을 직접 참조하여 처리
        var myEntity = _combatSystem.GetEntityById(_ownerIdx);
        if (myEntity == null) return;

        // 발동 로그
        Debug.Log($"<color=cyan><b>[Passive Activated!]</b></color> Nikke Index {_ownerIdx} - {_skillData.name} Triggered!");

        // TODO: 향후 CombatSystem의 실질적인 함수 (ApplyBuff 등)를 직접 호출
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}

