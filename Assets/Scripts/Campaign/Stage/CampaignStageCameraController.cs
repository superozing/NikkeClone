using Unity.Cinemachine;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

/// <summary>
/// CampaignStage의 Combat용 Cinemachine 카메라를 관리합니다.
/// Combat 상태 진입/종료 시 CameraManager의 Activate/Deactivate를 사용하여 카메라 전환을 제어합니다.
/// </summary>
public class CampaignStageCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cam;

    // 설계서 Section: Priority Layer 정의 - Stage: 100
    private const int STAGE_CAMERA_PRIORITY = 100;

    private string _camKey;

    private void OnEnable()
    {
        if (_cam == null)
        {
            Debug.LogError($"[CampaignStageCameraController] _cam이 null입니다. GameObject={gameObject.name}");
            return;
        }
    }

    private void OnDisable()
    {
        if (string.IsNullOrEmpty(_camKey))
        {
            Debug.LogWarning($"[CampaignStageCameraController] _camKey가 비어있어 카메라 등록을 해제하지 않습니다.");
            return;
        }

        Managers.Camera.UnregisterCamera(_camKey);
    }

    /// <summary>
    /// 카메라 매니저에 스테이지 시네머신 카메라를 등록합니다.
    /// </summary>
    /// <param name="_stageId"></param>
    public void SetStageId(int _stageId)
    {
        _camKey = $"CAM_STAGE{_stageId}";

        // Stage 카메라를 Priority 100으로 등록 (비활성 상태)
        Managers.Camera.RegisterCamera(_camKey, _cam, STAGE_CAMERA_PRIORITY);
    }

    /// <summary>
    /// Combat 상태 진입 시 카메라를 활성화합니다.
    /// CameraManager의 Activate를 사용하여 등록된 Priority(100)가 적용됩니다.
    /// </summary>
    public void ActivateCombatCamera()
    {
        Managers.Camera.Activate(_camKey);
    }

    /// <summary>
    /// Combat 상태 종료 시 카메라를 비활성화합니다.
    /// CameraManager의 Deactivate를 사용하여 Priority가 0으로 설정됩니다.
    /// </summary>
    public void DeactivateCombatCamera()
    {
        Managers.Camera.Deactivate(_camKey);
    }
}
