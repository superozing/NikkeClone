using UnityEngine;

/// <summary>
/// 스테이지 대기 상태입니다.
/// 기본 상태로 스쿼드와의 충돌을 대기합니다.
/// </summary>
public class StageIdleState : IState<CampaignStage>
{
    /// <summary>
    /// Idle 상태 진입 시 호출됩니다.
    /// </summary>
    public void Enter(CampaignStage owner)
    {
        // TODO:
        // 1. idle 애니메이션 재생

        Debug.Log($"[StageIdleState] Enter - 스테이지 {owner.StageId} 대기 상태 진입");
    }

    /// <summary>
    /// Idle 상태에서 매 프레임 호출됩니다.
    /// </summary>
    public void Execute(CampaignStage owner)
    {
        // 대기 상태에서는 특별한 로직 없음.
        // 충돌 감지는 OnTriggerEnter에서 처리.
    }

    /// <summary>
    /// Idle 상태 종료 시 호출됩니다.
    /// </summary>
    public void Exit(CampaignStage owner)
    {
        Debug.Log($"[StageIdleState] Exit - 스테이지 {owner.StageId} 대기 상태 종료");
    }
}
