using UnityEngine;

/// <summary>
/// 스쿼드 대기 상태입니다.
/// 정지 상태로 유지하며 사용자 입력(맵 클릭)을 대기합니다.
/// </summary>
/// <remarks>
/// Implements Section 3.1: SquadIdleState (CampaignUnit_Design.md)
/// </remarks>
public class SquadIdleState : IState<CampaignSquad>
{
    /// <summary>
    /// Idle 상태 진입 시 호출됩니다.
    /// NavMeshAgent를 정지시키고 대기 상태로 설정합니다.
    /// </summary>
    public void Enter(CampaignSquad owner)
    {
        owner.Agent.isStopped = true;
        owner.Agent.velocity = Vector3.zero;
        Debug.Log("[SquadIdleState] Enter - 대기 상태 진입");
    }

    /// <summary>
    /// Idle 상태에서 매 프레임 호출됩니다.
    /// 현재는 특별한 처리 없음.
    /// </summary>
    public void Execute(CampaignSquad owner)
    {
        // 대기 상태에서는 특별한 로직 없음.
        // 입력 처리는 CampaignScene에서 담당.
    }

    /// <summary>
    /// Idle 상태 종료 시 호출됩니다.
    /// </summary>
    public void Exit(CampaignSquad owner)
    {
        Debug.Log("[SquadIdleState] Exit - 대기 상태 종료");
    }
}
