using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Zone 트리거 영역에 Squad가 진입/이탈 시 해당 Zone의 카메라를 활성화/비활성화합니다.
/// Priority 50으로 등록되어 Squad 기본 카메라(20)보다 우선순위를 가집니다.
/// </summary>
public class CampaignZoneCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _cam;
    [SerializeField] private string _triggerTag = "Player";

    private const int ZONE_CAMERA_PRIORITY = 50;
    private string _camKey;

    private void OnEnable()
    {
        if (_cam == null)
        {
            Debug.LogError($"[CampaignZoneCameraController] _cam이 설정되지 않았습니다. GameObject: {gameObject.name}");
            return;
        }

        // Zone 카메라 키 생성: ZONE_{InstanceID}
        _camKey = $"ZONE_{GetInstanceID()}";

        // CameraManager에 Priority 50으로 등록
        Managers.Camera.RegisterCamera(_camKey, _cam, ZONE_CAMERA_PRIORITY);
    }

    private void OnDisable()
    {
        if (!string.IsNullOrEmpty(_camKey))
        {
            Managers.Camera.UnregisterCamera(_camKey);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (!other.CompareTag(_triggerTag)) return;

        // Squad가 진입하면 카메라 활성화
        Managers.Camera.Activate(_camKey);
        Debug.Log($"[CampaignZoneCameraController] Zone 진입: {_camKey} 활성화");
    }

    private void OnTriggerExit(Collider other)
    {
        //if (!other.CompareTag(_triggerTag)) return;

        // Squad가 이탈하면 카메라 비활성화 (기본 카메라로 복귀)
        Managers.Camera.Deactivate(_camKey);
        Debug.Log($"[CampaignZoneCameraController] Zone 이탈: {_camKey} 비활성화");
    }
}
