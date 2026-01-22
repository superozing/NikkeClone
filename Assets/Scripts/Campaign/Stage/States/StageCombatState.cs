using UnityEngine;

/// <summary>
/// 스테이지 전투 상태입니다.
/// 전투 애니메이션을 재생하고 CameraManager를 통해 카메라를 전환합니다.
/// </summary>
public class StageCombatState : IState<CampaignStage>
{
    /// <summary>
    /// Combat 상태 진입 시 호출됩니다.
    /// 카메라를 스테이지에 포커스합니다.
    /// </summary>
    public void Enter(CampaignStage owner)
    {
        // TODO: 전투 상태 애니메이션 실행

        // CameraController를 통해 Combat 카메라 활성화 (Priority 100)
        owner.CameraController.ActivateCombatCamera();

        Debug.Log($"[StageCombatState] Enter - 스테이지 {owner.StageId} 전투 상태 진입");
    }

    /// <summary>
    /// Combat 상태에서 매 프레임 호출됩니다.
    /// </summary>
    public void Execute(CampaignStage owner)
    {
        // 전투 상태에서는 특별한 로직 없음.
    }

    /// <summary>
    /// Combat 상태 종료 시 호출됩니다.
    /// </summary>
    public void Exit(CampaignStage owner)
    {
        // TODO: 전투 상태 애니메이션 종료

        // CameraController를 통해 Combat 카메라 비활성화 (Priority 10)
        // Squad 카메라가 다시 최고 Priority가 됨
        owner.CameraController.DeactivateCombatCamera();

        Debug.Log($"[StageCombatState] Exit - 스테이지 {owner.StageId} 전투 상태 종료");
    }
}
