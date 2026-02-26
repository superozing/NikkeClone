using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 중앙 집중형 타겟팅 시스템.
/// CombatSystem이 소유하며, 주기적으로 RaptureField의 Zone별
/// 최소 HP 랩쳐를 캐싱합니다.
/// </summary>
public class CombatTargetingSystem
{
    private RaptureField _raptureField;
    private Dictionary<eRangeZone, CombatRapture> _cachedTargets;

    // 캐싱된 Zone 배열
    private RaptureZone[] _nearZones;
    private RaptureZone[] _midZones;
    private RaptureZone[] _farZones;

    private float _refreshInterval = 0.5f;
    private float _refreshTimer;

    /// <summary>
    /// Caller: CombatSystem.InitializeAsync()
    /// Intent: RaptureField 참조 저장 + 캐시 딕셔너리 초기화
    /// </summary>
    public void Initialize(RaptureField raptureField)
    {
        _raptureField = raptureField;
        _cachedTargets = new Dictionary<eRangeZone, CombatRapture>
        {
            { eRangeZone.Near, null },
            { eRangeZone.Mid, null },
            { eRangeZone.Far, null },
        };

        // RaptureField로부터 각 Zone 배열을 미리 가져와 캐싱
        _nearZones = _raptureField.GetZones(eRangeZone.Near);
        _midZones = _raptureField.GetZones(eRangeZone.Mid);
        _farZones = _raptureField.GetZones(eRangeZone.Far);

        RefreshTargets();
        Debug.Log("[TargetingSystem] Initialized");
    }

    /// <summary>
    /// Caller: CombatSystem.Update()
    /// Intent: _refreshInterval 간격으로 캐시 갱신
    /// </summary>
    public void Tick(float deltaTime)
    {
        _refreshTimer += deltaTime;
        if (_refreshTimer >= _refreshInterval)
        {
            _refreshTimer = 0f;
            RefreshTargets();
        }
    }

    /// <summary>
    /// 지정 Zone의 캐시된 최우선 타겟(최소 HP) 반환. null 가능.
    /// </summary>
    /// Caller: NikkeAutoAttackState.Execute()
    public CombatRapture GetTarget(eRangeZone zone)
    {
        if (_cachedTargets.TryGetValue(zone, out var target))
        {
            // 반환 전 유효성 재확인 (사망했을 수 있음)
            if (target != null && !target.IsDead)
                return target;
        }

        // 해당 구역에 타겟이 없다면 다른 구역 타겟 반환 시도
        foreach (var kvp in _cachedTargets)
        {
            if (kvp.Key == zone) continue;

            var fallbackTarget = kvp.Value;
            if (fallbackTarget != null && !fallbackTarget.IsDead)
                return fallbackTarget;
        }

        return null;
    }

    /// <summary>
    /// Near/Mid/Far 각 Zone을 순회하며 최소 HP 랩쳐를 캐싱합니다.
    /// </summary>
    private void RefreshTargets()
    {
        if (_raptureField == null) return;

        RefreshZone(eRangeZone.Near);
        RefreshZone(eRangeZone.Mid);
        RefreshZone(eRangeZone.Far);
    }

    private void RefreshZone(eRangeZone zoneType)
    {
        RaptureZone[] zones = zoneType switch
        {
            eRangeZone.Near => _nearZones,
            eRangeZone.Mid => _midZones,
            eRangeZone.Far => _farZones,
            _ => null
        };

        if (zones == null) return;

        CombatRapture minHpRapture = null;
        long minHp = long.MaxValue;

        foreach (var zone in zones)
        {
            foreach (var rapture in zone.Raptures)
            {
                if (rapture == null || rapture.IsDead) continue;
                if (rapture.CurrentHp < minHp)
                {
                    minHp = rapture.CurrentHp;
                    minHpRapture = rapture;
                }
            }
        }
        _cachedTargets[zoneType] = minHpRapture;
    }
}
