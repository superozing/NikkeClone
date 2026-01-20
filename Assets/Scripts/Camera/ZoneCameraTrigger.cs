using UnityEngine;

/// <summary>
/// 트리거 Zone 진입 시 Squad 카메라 인덱스를 변경합니다.
/// Zone 이탈 시 별도 동작 없이, 다른 Zone 진입 시 새 인덱스로 덮어씌워집니다.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZoneCameraTrigger : MonoBehaviour
{
    [Header("Squad Camera Index")]
    [SerializeField] private int _squadCameraIndex = -1;

    [Header("Trigger Settings")]
    [SerializeField] private string TriggerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(TriggerTag)) return;
        if (_squadCameraIndex < 0) return;

        CampaignSquad squad = other.GetComponentInParent<CampaignSquad>();
        if (squad != null && squad.CameraController != null)
        {
            squad.CameraController.SetActiveCameraIndex(_squadCameraIndex);
        }
    }
}
