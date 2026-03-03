using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 버스트 시스템의 패시브 트리거 호출을 테스트하기 위한 빈 버스트 스킬입니다.
/// </summary>
public class Burst_Test : Burst_Base
{
    protected override void ExecuteSkill()
    {
        Debug.Log($"<color=magenta>[Burst_Test]</color> 스킬이 발동되었습니다! (OwnerIndex: {_ownerIdx}, BurstStage: {(eBurstStage)_skillData.burstStage})");
    }
}
