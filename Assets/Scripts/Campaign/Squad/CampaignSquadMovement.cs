using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class CampaignSquadMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CampaignSquad _squad;
    [SerializeField] private Camera _mainCamera;

    [Header("Settings")]
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private float _navMeshSampleDistance = 1f;

    private void Awake()
    {
        if (_squad == null)
            _squad = GetComponent<CampaignSquad>();

        if (_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        // 씬 스크립트에서 기본 액션맵을 Campaign으로 설정해야 해요.
        Managers.Input.BindAction("Click", OnClick);
    }

    private void OnDisable()
    {
        Managers.Input.UnbindAction("Click", OnClick);
    }

    private void OnClick(InputAction.CallbackContext context)
    {
        // UI 위 클릭 시 무시
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // 마우스 위치에서 Ray 생성
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = _mainCamera.ScreenPointToRay(mousePosition);

        // Ground 레이어에 대한 Raycast 수행
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _groundLayerMask))
        {
            return;
        }

        // NavMesh 위의 유효한 위치인지 확인
        if (!NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, _navMeshSampleDistance, NavMesh.AllAreas))
        {
            Debug.Log($"[CampaignSquadMovement] NavMesh.SamplePosition 실패 - 위치: {hit.point}");
            return;
        }

        // 스쿼드 이동 명령
        Debug.Log($"[CampaignSquadMovement] 이동 명령 - 목적지: {navHit.position}");
        _squad.MoveTo(navHit.position);
    }
}
