using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZoneCameraTrigger : MonoBehaviour
{
    /// <summary>
    /// 진입 시 전환할 카메라 키
    /// </summary>
    [SerializeField]
    private string CameraKey;

    /// <summary>
    /// 카메라 전환 시간 (초)
    /// </summary>
    [SerializeField]
    private float BlendTime = 0.5f;

    /// <summary>
    /// Zone 이탈 시 복귀할 카메라 키 (설정하지 않으면 이전 카메라로 복귀)
    /// </summary>
    [SerializeField]
    private string FallbackCameraKey = "Operation_Follow";

    /// <summary>
    /// 트리거 감지 대상 태그 (예: "Player")
    /// </summary>
    [SerializeField]
    private string TriggerTag = "Player";

    private string _previousCameraKey;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TriggerTag))
        {
            return;
        }

        if (string.IsNullOrEmpty(CameraKey))
        {
            Debug.LogWarning($"[ZoneCameraTrigger] CameraKey가 설정되지 않았습니다. GameObject={gameObject.name}");
            return;
        }

        // 현재 카메라 키 저장 (이탈 시 복귀용)
        _previousCameraKey = Managers.Camera.GetCurrentCameraKey();

        // Zone 카메라로 전환
        Managers.Camera.SwitchTo(CameraKey, BlendTime);
        Debug.Log($"[ZoneCameraTrigger] Zone 진입: {CameraKey}");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(TriggerTag))
        {
            return;
        }

        // 이전 카메라 또는 Fallback 카메라로 복귀
        string targetKey = !string.IsNullOrEmpty(_previousCameraKey) ? _previousCameraKey : FallbackCameraKey;
        Managers.Camera.SwitchTo(targetKey, BlendTime);
        Debug.Log($"[ZoneCameraTrigger] Zone 이탈: {targetKey}로 복귀");

        _previousCameraKey = null;
    }
}
