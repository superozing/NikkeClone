using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 하나의 거리 구역입니다.
/// 여러 스폰 위치를 가질 수 있으며, 랩쳐는 구역 내에서 가로 이동합니다.
/// </summary>
public class RaptureZone : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private eRangeZone _zoneType;

    [Header("Spawn Positions - 챕터별로 다를 수 있음")]
    [SerializeField] private Transform[] _spawnPositions;

    [Header("Movement Bounds")]
    [SerializeField] private float _leftBound;
    [SerializeField] private float _rightBound;

    // ==================== TEST CODE: Random Spawn Bounds ====================
    // TODO: 정식 스폰 시스템 구현 후 제거
    // 제거 시: 아래 필드, GetRandomSpawnPosition(), OnDrawGizmosSelected() 삭제
    [Header("[TEST] Random Spawn Bounds")]
    [SerializeField] private Vector3 _boundsCenter = Vector3.zero;
    [SerializeField] private Vector3 _boundsSize = new Vector3(5f, 0f, 3f);
    // ==================== END TEST CODE ====================

    /// <summary>구역 타입</summary>
    public eRangeZone ZoneType => _zoneType;

    /// <summary>스폰 가능 위치들</summary>
    public Transform[] SpawnPositions => _spawnPositions;

    /// <summary>이동 가능 범위 (가로)</summary>
    public (float left, float right) MovementBounds => (_leftBound, _rightBound);

    /// <summary>현재 구역에 있는 랩쳐들</summary>
    public List<CombatRapture> Raptures { get; } = new List<CombatRapture>();

    // Phase 4: WaveSystem에서 랩쳐 스폰 시 호출
    // Caller: WaveSystem.SpawnRapture()
    public void AddRapture(CombatRapture rapture)
    {
        if (!Raptures.Contains(rapture))
            Raptures.Add(rapture);
    }

    // Phase 4: 랩쳐 사망/이동 시 호출
    // Caller: CombatRapture.Die() 또는 SetZone() (via WaveSystem)
    public void RemoveRapture(CombatRapture rapture)
    {
        if (Raptures.Contains(rapture))
            Raptures.Remove(rapture);
    }

    // ==================== TEST CODE: Random Spawn ====================
    // TODO: 정식 스폰 시스템 구현 후 제거

    /// <summary>
    /// [TEST] Bounds 영역 내 랜덤 월드 좌표를 반환합니다.
    /// Caller: RaptureField.GetRandomSpawnPosition()
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        Vector3 localPos = new Vector3(
            UnityEngine.Random.Range(-_boundsSize.x / 2f, _boundsSize.x / 2f),
            0f, // Y축은 0 고정 (지상 랩쳐)
            UnityEngine.Random.Range(-_boundsSize.z / 2f, _boundsSize.z / 2f)
        );

        // Center 오프셋 + Zone Transform 기준 월드 좌표 변환
        return transform.TransformPoint(_boundsCenter + localPos);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(_boundsCenter, _boundsSize);
    }
#endif
    // ==================== END TEST CODE ====================
}
