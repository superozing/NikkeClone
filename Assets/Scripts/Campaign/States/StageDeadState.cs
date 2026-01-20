using UnityEngine;

/// <summary>
/// 스테이지 사망 상태입니다.
/// 사망 애니메이션을 재생하고 오브젝트를 비활성화합니다.
/// </summary>
/// <remarks>
/// Implements Section 4.3: StageDeadState (CampaignUnit_Design.md)
/// </remarks>
public class StageDeadState : IState<CampaignStage>
{
    /// <summary>
    /// Dead 상태 진입 시 호출됩니다.
    /// 스테이지 오브젝트를 비활성화합니다.
    /// </summary>
    public void Enter(CampaignStage owner)
    {
        Debug.Log($"[StageDeadState] Enter - 스테이지 {owner.StageId} 사망 상태 진입");

        // TODO:
        // 1. 사망 애니메이션 재생 후 비활성화

        // 임시로 즉시 비활성화
        owner.gameObject.SetActive(false);
    }

    /// <summary>
    /// Dead 상태에서 매 프레임 호출됩니다.
    /// 비활성화되므로 호출되지 않음.
    /// </summary>
    public void Execute(CampaignStage owner)
    {
        // 비활성화 상태이므로 실행되지 않음.
    }

    /// <summary>
    /// Dead 상태 종료 시 호출됩니다.
    /// </summary>
    public void Exit(CampaignStage owner)
    {
        // 사망 상태에서는 일반적으로 Exit가 호출되지 않음.
        Debug.Log($"[StageDeadState] Exit - 스테이지 {owner.StageId} 사망 상태 종료");
    }
}
