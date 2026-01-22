using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// CampaignSquad의 기본 Cinemachine 카메라를 관리합니다.
/// Priority 20으로 설정되어 가장 낮은 우선순위를 가집니다.
/// Zone 카메라(50)나 Stage Combat 카메라(100)가 없을 때 활성화됩니다.
/// </summary>
public class CampaignSquadCameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera _cam;

    private const int DEFAULT_CAM_PRIORITY = 20;
    private const string CAM_KEY = "CAM_SQUAD";

    private void OnEnable()
    {
        if (_cam == null)
        {
            Debug.LogError("[CampaignSquadCameraController] _cam이 설정되지 않았습니다.");
            return;
        }

        // 기본 카메라를 Priority 20으로 등록
        Managers.Camera.RegisterCamera(CAM_KEY, _cam, DEFAULT_CAM_PRIORITY);

        // 항상 활성화 상태 유지 (다른 높은 Priority 카메라가 없을 때만 보임)
        Managers.Camera.Activate(CAM_KEY);
    }

    private void OnDisable()
    {
        Managers.Camera.UnregisterCamera(CAM_KEY);
    }
}
