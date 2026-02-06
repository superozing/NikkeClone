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
    // Note: CombatRapture 클래스는 Phase 2에서 구현되므로 여기서는 주석 처리하거나 빈 리스트만 유지
    // Phase 1: 컴파일 에러 방지를 위해 object로 대체하거나 나중에 추가
    // public List<CombatRapture> Raptors { get; } = new List<CombatRapture>();

    // Phase 1 Interim:
    public List<Transform> Raptors { get; } = new List<Transform>();

    /*
    /// <summary>랩쳐를 구역에 추가</summary>
    public void AddRaptor(CombatRapture raptor)
    {
        if (!Raptors.Contains(raptor))
            Raptors.Add(raptor);
    }
    
    /// <summary>랩쳐를 구역에서 제거</summary>
    public void RemoveRaptor(CombatRapture raptor)
    {
        Raptors.Remove(raptor);
    }
    */
}
