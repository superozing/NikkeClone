using UnityEngine;

/// <summary>
/// 캠페인 맵에서 적 스테이지를 나타내는 유닛입니다.
/// StateMachine을 통해 Idle, Combat, Dead 상태를 관리합니다.
/// </summary>
public class CampaignStage : MonoBehaviour
{
    [Header("Stage Info")]
    [SerializeField] private int _stageId;

    [Header("Components")]
    [SerializeField] private Collider _triggerCollider;
    [SerializeField] private CampaignStageCameraController _cameraController;

    /// <summary>
    /// Stage의 카메라 컨트롤러를 반환합니다.
    /// StageCombatState에서 카메라 Priority 제어에 사용합니다.
    /// </summary>
    public CampaignStageCameraController CameraController => _cameraController;

    private StateMachine<CampaignStage> _stateMachine;

    /// <summary>
    /// 스테이지 ID를 반환합니다.
    /// </summary>
    public int StageId => _stageId;

    /// <summary>
    /// 스테이지의 전방 방향을 반환합니다.
    /// 전투 종료 후 스쿼드 이동 방향으로 사용됩니다.
    /// </summary>
    public Vector3 ForwardDirection => transform.forward;

    private void Awake()
    {
        _stateMachine = new StateMachine<CampaignStage>(this);
    }

    private void Start()
    {
        // 초기 상태: Idle
        _stateMachine.ChangeState(new StageIdleState());
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    /// <summary>
    /// 스테이지 ID를 설정합니다.
    /// CampaignScene.Init()에서 호출됩니다.
    /// </summary>
    /// <param name="stageId">설정할 스테이지 ID</param>
    public void SetStageId(int stageId)
    {
        _stageId = stageId;

        // id 값을 사용하여 카메라매니저에 카메라 등록해요.
        _cameraController.SetStageId(_stageId);

        Debug.Log($"[CampaignStage] SetStageId - StageId: {_stageId}");
    }

    /// <summary>
    /// 스테이지를 전투 상태로 전환합니다.
    /// 스쿼드와 충돌 시 호출됩니다.
    /// </summary>
    public void EnterCombat()
    {
        _stateMachine.ChangeState(new StageCombatState());
    }

    /// <summary>
    /// 전투 상태를 종료하고 Idle 상태로 복귀합니다.
    /// </summary>
    public void ExitCombat()
    {
        _stateMachine.ChangeState(new StageIdleState());
    }

    /// <summary>
    /// 스테이지를 사망 상태로 전환합니다.
    /// 전투 승리 후 호출됩니다.
    /// </summary>
    public void Die()
    {
        _stateMachine.ChangeState(new StageDeadState());
    }
}
