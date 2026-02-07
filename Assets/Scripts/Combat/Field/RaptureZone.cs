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

    /// <summary>구역 타입</summary>
    public eRangeZone ZoneType => _zoneType;

    /// <summary>스폰 가능 위치들</summary>
    public Transform[] SpawnPositions => _spawnPositions;

    /// <summary>이동 가능 범위 (가로)</summary>
    public (float left, float right) MovementBounds => (_leftBound, _rightBound);

    /// <summary>현재 구역에 있는 랩쳐들</summary>
    public List<CombatRapture> Raptures { get; } = new List<CombatRapture>();

    // Phase 4: WaveManager에서 랩쳐 스폰 시 호출
    // public void AddRapture(CombatRapture rapture)
    // Caller: WaveManager.SpawnRapture()

    // Phase 4: 랩쳐 사망/이동 시 호출
    // public void RemoveRapture(CombatRapture rapture)
    // Caller: CombatRapture.Die() 또는 SetZone()
}
