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
        Debug.Log($"[StageCombatState] Enter - 스테이지 {owner.StageId} 전투 상태 진입");

        // TODO:
        // 1. 전투 상태 애니메이션 실행
        // 2. 시선벡터를 CampaignStage 방향으로 전환
        // 3. 시네머신 카메라를 자신으로 설정

        // CameraManager를 통한 카메라 전환
        // Managers.Camera.SwitchTo("StageEngage", owner.transform);
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
        // TODO:
        // 1. 전투 상태 애니메이션 종료
        // 2. 시선벡터를 기존 방향으로 변경
        // 3. 시네머신 카메라를 스쿼드로 설정(여기서 해주는 게 좋지 않아보여요. 스쿼드가 설정하도록 해야 할 듯)

        Debug.Log($"[StageCombatState] Exit - 스테이지 {owner.StageId} 전투 상태 종료");
    }
}
