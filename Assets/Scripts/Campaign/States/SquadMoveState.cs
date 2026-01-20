using UnityEngine;

/// <summary>
/// 스쿼드 이동 상태입니다.
/// NavMeshAgent를 통해 목적지로 이동하고, 도착 시 Idle 상태로 전환합니다.
/// </summary>
public class SquadMoveState : IState<CampaignSquad>
{
    private readonly Vector3 _destination;

    /// <summary>
    /// 이동 상태를 생성합니다.
    /// </summary>
    /// <param name="destination">이동 목적지 좌표</param>
    public SquadMoveState(Vector3 destination)
    {
        _destination = destination;
    }

    /// <summary>
    /// Move 상태 진입 시 호출됩니다.
    /// NavMeshAgent를 활성화하고 목적지를 설정합니다.
    /// </summary>
    public void Enter(CampaignSquad owner)
    {
        owner.Agent.isStopped = false;
        owner.Agent.SetDestination(_destination);
        Debug.Log($"[SquadMoveState] Enter - 목적지: {_destination}");
    }

    /// <summary>
    /// Move 상태에서 매 프레임 호출됩니다.
    /// 목적지 도착 여부를 확인하고, 도착 시 Idle 상태로 전환합니다.
    /// </summary>
    public void Execute(CampaignSquad owner)
    {
        // 경로 계산 완료 및 목적지 도착 확인
        if (!owner.Agent.pathPending && owner.Agent.remainingDistance <= owner.Agent.stoppingDistance)
        {
            // 속도가 거의 0일 때 도착으로 판정
            if (!owner.Agent.hasPath || owner.Agent.velocity.sqrMagnitude < 0.01f)
            {
                Debug.Log("[SquadMoveState] Execute - 목적지 도착, Idle 상태로 전환");
                owner.TransitionToIdle();
            }
        }
    }

    /// <summary>
    /// Move 상태 종료 시 호출됩니다.
    /// NavMeshAgent 이동을 중지합니다.
    /// </summary>
    public void Exit(CampaignSquad owner)
    {
        owner.Agent.isStopped = true;
        Debug.Log("[SquadMoveState] Exit - 이동 상태 종료");
    }
}
