using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Cinemachine 카메라를 Layer(Priority) 기반으로 등록 및 관리합니다.
/// Activate/Deactivate 방식으로 카메라 전환을 제어합니다.
/// </summary>
public class CameraManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Camera;

    #region Internal Data Structure

    /// <summary>
    /// 등록된 카메라의 상태를 관리하는 내부 클래스입니다.
    /// </summary>
    private class CameraEntry
    {
        public CinemachineCamera Camera;
        public int RegisteredPriority;
        public bool IsActive;
    }

    #endregion

    // _brain 캐싱
    private CinemachineBrain _brain;

    // _cameras Dictionary
    private Dictionary<string, CameraEntry> _cameras = new Dictionary<string, CameraEntry>();

    // _currentCameraKey (Activate된 카메라들 중 가장 높은 Priority의 카메라)
    private string _currentCameraKey;

    // Impulse Source
    private CinemachineImpulseSource _impulseSource;

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

    #region 카메라 등록/해제

    /// <summary>
    /// Virtual Camera를 CameraManager에 등록합니다.
    /// 등록 시 우선순위를 지정하며, 카메라는 비활성 상태(Priority=0)로 등록됩니다.
    /// </summary>
    /// <param name="key">고유 식별 키</param>
    /// <param name="camera">등록할 CinemachineCamera</param>
    /// <param name="priority">카메라 활성화 시 적용될 우선순위</param>
    public void RegisterCamera(string key, CinemachineCamera camera, int priority)
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

        _cameras[key] = new CameraEntry
        {
            Camera = camera,
            RegisteredPriority = priority,
            IsActive = false
        };
        camera.Priority = 0; // 비활성 상태로 등록
        Debug.Log($"[CameraManager] Camera 등록: {key} (Priority={priority})");
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

    #region 카메라 활성화/비활성화

    /// <summary>
    /// 카메라를 활성화합니다. 등록된 우선순위가 적용됩니다.
    /// </summary>
    /// <param name="key">활성화할 카메라의 키</param>
    /// <param name="blendTime">전환 시간 (초). null이면 Brain 기본값 사용</param>
    public void Activate(string key, float? blendTime = null)
    {
        if (!_cameras.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"[CameraManager] Activate: key={key}가 등록되어 있지 않습니다.");
            return;
        }

        entry.IsActive = true;
        entry.Camera.Priority = entry.RegisteredPriority;

        if (blendTime.HasValue)
        {
            GetBrain().DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseInOut,
                blendTime.Value
            );
        }

        Debug.Log($"[CameraManager] Activate: {key} (Priority={entry.RegisteredPriority})");
    }

    /// <summary>
    /// 카메라를 비활성화합니다. Priority가 0으로 설정됩니다.
    /// </summary>
    /// <param name="key">비활성화할 카메라의 키</param>
    public void Deactivate(string key)
    {
        if (!_cameras.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"[CameraManager] Deactivate: key={key}가 등록되어 있지 않습니다.");
            return;
        }

        entry.IsActive = false;
        entry.Camera.Priority = 0;

        Debug.Log($"[CameraManager] Deactivate: {key}");
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

        if (!_cameras.TryGetValue(key, out var entry))
        {
            Debug.LogWarning($"[CameraManager] TryGetCamera: key={key}가 등록되어 있지 않습니다.");
            camera = null;
            return false;
        }

        camera = entry.Camera;
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
