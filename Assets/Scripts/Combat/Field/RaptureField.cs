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

    /* Phase 3에서 사용 예정
    /// <summary>해당 거리의 모든 구역 조회</summary>
    public RaptureZone[] GetZones(eRangeZone zone) => zone switch
    {
        eRangeZone.Near => _nearZones,
        eRangeZone.Mid => _midZones,
        eRangeZone.Far => _farZones,
        _ => System.Array.Empty<RaptureZone>()
    };
    */

    /* Phase 2: CombatRapture 구현 후 활성화
    /// <summary>모든 구역의 랩쳐 목록</summary>
    public List<CombatRapture> GetAllRaptors()
    {
        var all = new List<CombatRapture>();
        all.AddRange(_nearZone.Raptors);
        all.AddRange(_midZone.Raptors);
        all.AddRange(_farZone.Raptors);
        return all;
    }
    */
}
