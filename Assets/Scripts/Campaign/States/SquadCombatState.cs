using UnityEngine;

/// <summary>
/// 스쿼드 전투 상태입니다.
/// 이동을 중지하고 전투 UI를 생성합니다.
/// </summary>
public class SquadCombatState : IState<CampaignSquad>
{
    private readonly CampaignStage _targetStage;

    /// <summary>
    /// 전투 상태를 생성합니다.
    /// </summary>
    /// <param name="targetStage">전투 대상 스테이지</param>
    public SquadCombatState(CampaignStage targetStage)
    {
        _targetStage = targetStage;
    }

    /// <summary>
    /// Combat 상태 진입 시 호출됩니다.
    /// NavMeshAgent를 정지시키고 전투 UI를 표시합니다.
    /// </summary>
    public void Enter(CampaignSquad owner)
    {
        owner.Agent.isStopped = true;
        owner.Agent.velocity = Vector3.zero;
        Debug.Log($"[SquadCombatState] Enter - 스테이지 {_targetStage.StageId}와 전투 개시");

        // TODO: 전투 UI 생성 (BattleInfoUI)
        // 흠... 여기서 생성하는 것 보다 스테이지가 생성하는 게 더 괜찮은 구조 같아요.
        // 왜냐하면 그렇게 되면 타겟 스테이지를 알지 않아도 동작하기 때문이에요.-
        // Managers.UI.ShowPopupUI<UI_BattleInfo>();
    }

    /// <summary>
    /// Combat 상태에서 매 프레임 호출됩니다.
    /// 전투 상태에서는 특별한 로직 없음 (UI 입력 대기).
    /// </summary>
    public void Execute(CampaignSquad owner)
    {
        // 전투 상태에서는 UI 입력을 대기합니다.
    }

    /// <summary>
    /// Combat 상태 종료 시 호출됩니다.
    /// </summary>
    public void Exit(CampaignSquad owner)
    {
        Debug.Log("[SquadCombatState] Exit - 전투 상태 종료");
    }

    /// <summary>
    /// 현재 전투 대상 스테이지를 반환합니다.
    /// </summary>
    public CampaignStage TargetStage => _targetStage;
}
