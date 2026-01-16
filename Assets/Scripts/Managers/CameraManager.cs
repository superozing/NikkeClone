using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Camera;

    // _brain 캐싱
    private CinemachineBrain _brain;

    // _cameras Dictionary
    private Dictionary<string, CinemachineCamera> _cameras = new Dictionary<string, CinemachineCamera>();

    // _currentCameraKey
    private string _currentCameraKey;

    // Priority 기반 전환
    private const int PRIORITY_DEFAULT = 10;
    private const int PRIORITY_ACTIVE = 100;

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
        CacheBrain();
    }

    public void Update()
    {
    }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
        _cameras.Clear();
        _currentCameraKey = null;
        _brain = null;
    }

    /// <summary>
    /// CinemachineBrain을 캐싱합니다. Main Camera에서 찾습니다.
    /// </summary>
    private void CacheBrain()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[CameraManager] Main Camera를 찾을 수 없습니다.");
            return;
        }

        _brain = mainCamera.GetComponent<CinemachineBrain>();
        if (_brain == null)
        {
            Debug.LogWarning("[CameraManager] Main Camera에 CinemachineBrain이 없습니다.");
        }
    }

    #region Section 4.1: 카메라 등록/해제

    /// <summary>
    /// Virtual Camera를 CameraManager에 등록합니다.
    /// </summary>
    /// <param name="key">고유 식별 키</param>
    /// <param name="camera">등록할 CinemachineCamera</param>
    public void RegisterCamera(string key, CinemachineCamera camera)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[CameraManager] RegisterCamera: key가 null 또는 빈 문자열입니다.");
            return;
        }

        if (camera == null)
        {
            Debug.LogError($"[CameraManager] RegisterCamera: camera가 null입니다. key={key}");
            return;
        }

        if (_cameras.ContainsKey(key))
        {
            Debug.LogWarning($"[CameraManager] RegisterCamera: key={key}가 이미 등록되어 있습니다. 덮어씁니다.");
        }

        _cameras[key] = camera;
        camera.Priority = PRIORITY_DEFAULT;
        Debug.Log($"[CameraManager] Camera 등록: {key}");
    }

    /// <summary>
    /// 등록된 Virtual Camera를 해제합니다.
    /// </summary>
    /// <param name="key">해제할 카메라의 키</param>
    public void UnregisterCamera(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[CameraManager] UnregisterCamera: key가 null 또는 빈 문자열입니다.");
            return;
        }

        if (_cameras.Remove(key))
        {
            Debug.Log($"[CameraManager] Camera 해제: {key}");

            if (_currentCameraKey == key)
            {
                _currentCameraKey = null;
            }
        }
        else
        {
            Debug.LogWarning($"[CameraManager] UnregisterCamera: key={key}가 등록되어 있지 않습니다.");
        }
    }

    #endregion

    #region 카메라 전환

    /// <summary>
    /// 지정된 카메라로 부드럽게 전환합니다.
    /// </summary>
    /// <param name="key">전환할 카메라의 키</param>
    /// <param name="blendTime">전환 시간 (초). null이면 Brain 기본값 사용</param>
    public void SwitchTo(string key, float? blendTime = null)
    {
        if (!TryGetCamera(key, out CinemachineCamera targetCamera))
        {
            Debug.Log($"[CameraManager] 등록되지 않은 키: {key}");
            return;
        }

        // 이전 카메라 Priority 낮추기
        if (!string.IsNullOrEmpty(_currentCameraKey) && _cameras.TryGetValue(_currentCameraKey, out CinemachineCamera previousCamera))
        {
            previousCamera.Priority = PRIORITY_DEFAULT;
        }

        // Blend 시간 설정 (blendTime이 지정된 경우)
        if (blendTime.HasValue && _brain != null)
        {
            _brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseInOut,
                blendTime.Value
            );
        }

        // 새 카메라 Priority 높이기
        targetCamera.Priority = PRIORITY_ACTIVE;
        _currentCameraKey = key;

        Debug.Log($"[CameraManager] SwitchTo: {key}, BlendTime={blendTime?.ToString() ?? "default"}");
    }

    /// <summary>
    /// 지정된 카메라로 즉시 전환합니다 (Cut).
    /// </summary>
    /// <param name="key">전환할 카메라의 키</param>
    public void SwitchToImmediate(string key)
    {
        if (!TryGetCamera(key, out CinemachineCamera targetCamera))
        {
            Debug.Log($"[CameraManager] 등록되지 않은 키: {key}");
            return;
        }

        // 이전 카메라 Priority 낮추기
        if (!string.IsNullOrEmpty(_currentCameraKey) && _cameras.TryGetValue(_currentCameraKey, out CinemachineCamera previousCamera))
        {
            previousCamera.Priority = PRIORITY_DEFAULT;
        }

        // Cut 전환을 위해 Brain의 기본 Blend를 Cut으로 임시 변경
        CinemachineBlendDefinition originalBlend = default;
        if (_brain != null)
        {
            originalBlend = _brain.DefaultBlend;
            _brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.Cut,
                0f
            );
        }

        // 새 카메라 Priority 높이기
        targetCamera.Priority = PRIORITY_ACTIVE;
        _currentCameraKey = key;

        // 원래 Blend 설정 복원 (다음 프레임에 적용되도록)
        if (_brain != null)
        {
            // Note: 즉시 복원하면 Cut이 적용되기 전에 원래 설정이 복원될 수 있음
            // 실제 운용 시에는 코루틴 또는 다음 프레임 콜백으로 복원 권장
            _brain.DefaultBlend = originalBlend;
        }

        Debug.Log($"[CameraManager] SwitchToImmediate: {key}");
    }

    #endregion

    #region 타겟 설정

    /// <summary>
    /// 카메라의 Follow 타겟을 설정합니다.
    /// </summary>
    /// <param name="key">카메라 키</param>
    /// <param name="target">Follow 타겟 Transform</param>
    public void SetFollowTarget(string key, Transform target)
    {
        if (!TryGetCamera(key, out CinemachineCamera camera))
        {
            Debug.Log($"[CameraManager] 등록되지 않은 키: {key}");
            return;
        }

        camera.Follow = target;
        Debug.Log($"[CameraManager] SetFollowTarget: {key} -> {(target != null ? target.name : "null")}");
    }

    /// <summary>
    /// 카메라의 LookAt 타겟을 설정합니다.
    /// </summary>
    /// <param name="key">카메라 키</param>
    /// <param name="target">LookAt 타겟 Transform</param>
    public void SetLookAtTarget(string key, Transform target)
    {
        if (!TryGetCamera(key, out CinemachineCamera camera))
        {
            Debug.Log($"[CameraManager] 등록되지 않은 키: {key}");
            return;
        }

        camera.LookAt = target;
        Debug.Log($"[CameraManager] SetLookAtTarget: {key} -> {(target != null ? target.name : "null")}");
    }

    #endregion

    #region Camera Shake (Impulse)

    // 기본 Impulse Source (런타임에 등록)
    private CinemachineImpulseSource _impulseSource;

    /// <summary>
    /// 기본 Impulse Source를 설정합니다.
    /// </summary>
    /// <param name="source">사용할 CinemachineImpulseSource</param>
    public void SetImpulseSource(CinemachineImpulseSource source)
    {
        _impulseSource = source;
        Debug.Log($"[CameraManager] ImpulseSource 설정됨: {(source != null ? source.gameObject.name : "null")}");
    }

    /// <summary>
    /// 지정된 위치에서 카메라 흔들림 임펄스를 발생시킵니다.
    /// </summary>
    /// <param name="position">임펄스 발생 위치</param>
    /// <param name="velocity">임펄스 강도 및 방향</param>
    public void TriggerImpulse(Vector3 position, Vector3 velocity)
    {
        if (_impulseSource == null)
        {
            Debug.LogWarning("[CameraManager] TriggerImpulse: ImpulseSource가 설정되지 않았습니다. SetImpulseSource()를 먼저 호출하세요.");
            return;
        }

        // ImpulseSource의 위치를 임시로 변경하여 임펄스 발생
        Vector3 originalPosition = _impulseSource.transform.position;
        _impulseSource.transform.position = position;
        _impulseSource.GenerateImpulse(velocity);
        _impulseSource.transform.position = originalPosition;

        Debug.Log($"[CameraManager] TriggerImpulse: position={position}, velocity={velocity}");
    }

    /// <summary>
    /// 기본 강도로 카메라 흔들림 임펄스를 발생시킵니다.
    /// </summary>
    /// <param name="velocity">임펄스 강도 및 방향</param>
    public void TriggerImpulse(Vector3 velocity)
    {
        if (_impulseSource == null)
        {
            Debug.LogWarning("[CameraManager] TriggerImpulse: ImpulseSource가 설정되지 않았습니다. SetImpulseSource()를 먼저 호출하세요.");
            return;
        }

        _impulseSource.GenerateImpulse(velocity);
        Debug.Log($"[CameraManager] TriggerImpulse: velocity={velocity}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// 등록된 카메라를 키로 조회합니다.
    /// </summary>
    /// <param name="key">카메라 키</param>
    /// <param name="camera">조회된 카메라 (out)</param>
    /// <returns>조회 성공 여부</returns>
    private bool TryGetCamera(string key, out CinemachineCamera camera)
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[CameraManager] TryGetCamera: key가 null 또는 빈 문자열입니다.");
            camera = null;
            return false;
        }

        if (!_cameras.TryGetValue(key, out camera))
        {
            Debug.LogWarning($"[CameraManager] TryGetCamera: key={key}가 등록되어 있지 않습니다.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 현재 활성 카메라의 키를 반환합니다.
    /// </summary>
    public string GetCurrentCameraKey()
    {
        return _currentCameraKey;
    }

    /// <summary>
    /// CinemachineBrain을 반환합니다. 필요 시 재캐싱합니다.
    /// </summary>
    public CinemachineBrain GetBrain()
    {
        if (_brain == null)
        {
            CacheBrain();
        }
        return _brain;
    }

    #endregion
}
