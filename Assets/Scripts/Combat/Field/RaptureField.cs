using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 랩쳐 필드를 관리합니다.
/// 세 가지 거리 구역(Near/Mid/Far)을 포함합니다.
/// </summary>
public class RaptureField : MonoBehaviour
{
    [Header("Range Zones")]
    [SerializeField] private RaptureZone[] _nearZones;
    [SerializeField] private RaptureZone[] _midZones;
    [SerializeField] private RaptureZone[] _farZones;

    /// <summary>해당 거리의 모든 구역 조회</summary>
    public RaptureZone[] GetZones(eRangeZone zone) => zone switch
    {
        eRangeZone.Near => _nearZones,
        eRangeZone.Mid => _midZones,
        eRangeZone.Far => _farZones,
        _ => System.Array.Empty<RaptureZone>()
    };

    /* Phase 2: CombatRapture 구현 후 활성화
    /// <summary>모든 구역의 랩쳐 목록</summary>
    public List<CombatRapture> GetAllRaptures()
    {
        var all = new List<CombatRapture>();
        all.AddRange(_nearZone.Raptures);
        all.AddRange(_midZone.Raptures);
        all.AddRange(_farZone.Raptures);
        return all;
    }
    */

    /// <summary>
    /// SpawnerId를 파싱하여 해당 스폰 위치를 반환합니다.
    /// Format: "{Zone}_{Air/Ground}_{Index}" (e.g., "Near_Ground_1")
    /// Caller: WaveSystem.SpawnRapture()
    /// </summary>
    public Transform GetSpawnPosition(string spawnerId)
    {
        // "Near_Ground_1" 형식 파싱
        var parts = spawnerId.Split('_');
        if (parts.Length < 3) return null;

        var zoneType = parts[0] switch
        {
            "Near" => eRangeZone.Near,
            "Mid" => eRangeZone.Mid,
            "Far" => eRangeZone.Far,
            _ => eRangeZone.Near
        };

        // Phase 4: Air/Ground 무시, index만 사용
        // index는 1-based라고 가정하고 0-based로 변환
        if (!int.TryParse(parts[2], out int index))
            index = 1;

        index -= 1;
        if (index < 0) index = 0;

        var zones = GetZones(zoneType);
        if (zones.Length == 0) return null;

        // 단순화: 첫 번째 구역의 해당 인덱스 스폰 위치 반환
        // (추후 구역이 여러 개라면 어떤 구역을 쓸지 결정하는 로직 필요)
        var targetZone = zones[0];
        if (index < targetZone.SpawnPositions.Length)
            return targetZone.SpawnPositions[index];

        // Fallback: 인덱스 초과 시 첫 번째 위치 반환
        return targetZone.SpawnPositions[0];
    }

    // ==================== TEST CODE: Random Spawn ====================
    // TODO: 정식 스폰 시스템 구현 후 제거

    /// <summary>
    /// [TEST] SpawnerId를 파싱하여 해당 Zone의 랜덤 스폰 위치를 반환합니다.
    /// Caller: WaveSystem.SpawnRapture()
    /// </summary>
    public Vector3 GetRandomSpawnPosition(string spawnerId)
    {
        eRangeZone zoneType = ParseZoneFromSpawnerId(spawnerId);
        var zones = GetZones(zoneType);

        if (zones == null || zones.Length == 0)
        {
            Debug.LogWarning($"[RaptureField] No zones found for: {spawnerId}");
            return Vector3.zero;
        }

        // 단순화: 첫 번째 Zone 사용
        return zones[0].GetRandomSpawnPosition();
    }

    private eRangeZone ParseZoneFromSpawnerId(string spawnerId)
    {
        if (string.IsNullOrEmpty(spawnerId)) return eRangeZone.Near;
        if (spawnerId.StartsWith("Near")) return eRangeZone.Near;
        if (spawnerId.StartsWith("Mid")) return eRangeZone.Mid;
        if (spawnerId.StartsWith("Far")) return eRangeZone.Far;
        return eRangeZone.Near;
    }
    // ==================== END TEST CODE ====================
}
