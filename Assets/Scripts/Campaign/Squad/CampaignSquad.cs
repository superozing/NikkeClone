using UnityEngine;
using UnityEngine.AI;

public class CampaignSquad : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private CampaignSquadCameraController _cameraController;

    /// <summary>
    /// Squad의 카메라 컨트롤러를 반환합니다.
    /// State 클래스에서 카메라 제어에 사용합니다.
    /// </summary>
    public CampaignSquadCameraController CameraController => _cameraController;

    private StateMachine<CampaignSquad> _stateMachine;
    private CampaignStage _currentTargetStage;

    /// <summary>
    /// State 클래스에서 이동 제어에 사용
    /// </summary>
    public NavMeshAgent Agent => _agent;

    /// <summary>
    /// 현재 전투 대상 스테이지를 반환합니다.
    /// </summary>
    public CampaignStage CurrentTargetStage => _currentTargetStage;

    private void Awake()
    {
        _stateMachine = new StateMachine<CampaignSquad>(this);
    }

    private void Start()
    {
        // 초기 상태: Idle
        _stateMachine.ChangeState(new SquadIdleState());
    }

    private void Update()
    {
        _stateMachine.Update();
    }

    /// <summary>
    /// 스쿼드를 지정된 목적지로 이동시킵니다.
    /// MoveState로 전환됩니다.
    /// </summary>
    /// <param name="destination">이동 목적지 좌표</param>
    public void MoveTo(Vector3 destination)
    {
        _stateMachine.ChangeState(new SquadMoveState(destination));
    }

    /// <summary>
    /// 스쿼드를 Idle 상태로 전환합니다.
    /// MoveState에서 목적지 도착 시 호출됩니다.
    /// </summary>
    public void TransitionToIdle()
    {
        _currentTargetStage = null;
        _stateMachine.ChangeState(new SquadIdleState());
    }

    /// <summary>
    /// 스쿼드를 전투 상태로 전환합니다.
    /// 스테이지와 충돌 시 호출됩니다.
    /// </summary>
    /// <param name="target">전투 대상 스테이지</param>
    public void EnterCombat(CampaignStage target)
    {
        _currentTargetStage = target;
        _stateMachine.ChangeState(new SquadCombatState(target));
    }

    /// <summary>
    /// 전투 상태를 종료하고 지정된 방향으로 이동합니다.
    /// UI에서 전투 종료 시 호출됩니다.
    /// </summary>
    /// <param name="moveDirection">이동할 방향 (스테이지의 Forward 방향 등)</param>
    public void ExitCombat(Vector3 moveDirection)
    {
        Vector3 destination = transform.position + moveDirection.normalized * 3f;
        _currentTargetStage = null;
        _stateMachine.ChangeState(new SquadMoveState(destination));
    }

    /// <summary>
    /// 스테이지와의 충돌 감지.
    /// CampaignStage와 충돌 시 양쪽 모두 Combat 상태로 전환합니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        CampaignStage stage = other.GetComponentInParent<CampaignStage>();
        if (stage == null) return;

        // 이미 전투 상태인 경우 무시
        if (_stateMachine.CurrentState is SquadCombatState) return;

        Debug.Log($"[CampaignSquad] OnTriggerEnter - 스테이지 {stage.StageId}와 충돌");
        EnterCombat(stage);
        stage.EnterCombat();
    }
}
