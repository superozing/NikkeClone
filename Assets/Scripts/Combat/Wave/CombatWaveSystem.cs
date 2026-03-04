using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 웨이브(페이즈) 기반으로 랩쳐 스폰을 관리하는 매니저입니다.
/// Phase 6: CombatSystem에 의해 관리되도록 리팩토링되었습니다.
/// </summary>
public class CombatWaveSystem : MonoBehaviour
{
    // ==================== Fields ====================

    [Header("Dependencies")]
    [SerializeField] private RaptureField _raptureField;

    private const string RAPTURE_PREFAB_KEY = "Prefabs/Combat/CombatRapture1";

    private StageBattleGameData _battleData;
    private int _currentPhaseIndex;
    private List<CombatRapture> _aliveRaptures = new List<CombatRapture>();
    private int _totalKilledCount;

    // Phase Progress
    private float _accumulatedWeight; // 이전 페이즈들 가중치 합
    private int _currentPhaseSpawnCount; // 현재 페이즈 총 스폰 수
    private int _currentPhaseKillCount; // 현재 페이즈 처치 수

    // ==================== Events ====================

    /// <summary>
    /// 페이즈 완료 시 발생 (페이즈 인덱스)
    /// </summary>
    public event Action<int> OnPhaseComplete;

    /// <summary>
    /// 모든 페이즈(전투) 완료 시 발생
    /// </summary>
    public event Action OnAllPhasesComplete;

    /// <summary>
    /// 랩쳐 사망 시 발생
    /// </summary>
    public event Action<CombatRapture> OnRaptureDied;

    // ==================== Properties ====================

    /// <summary>
    /// 현재 진행률 (0.0 ~ 1.0)
    /// </summary>
    public float Progress
    {
        get
        {
            if (_battleData == null) return 0f;

            // 현재 페이즈 진행률
            float currentPhaseWeight = 0f;
            if (_currentPhaseIndex < _battleData.phaseProgressWeights.Length)
                currentPhaseWeight = _battleData.phaseProgressWeights[_currentPhaseIndex];

            float currentPhaseProgress = 0f;
            if (_currentPhaseSpawnCount > 0)
                currentPhaseProgress = (float)_currentPhaseKillCount / _currentPhaseSpawnCount;

            return _accumulatedWeight + (currentPhaseProgress * currentPhaseWeight);
        }
    }

    public int AliveCount => _aliveRaptures.Count;

    // ==================== Public Methods ====================

    /// <summary>
    /// 동적 생성 시 의존성을 주입합니다.
    /// Caller: CombatSystem.InitializeAsync() (필요 시)
    /// </summary>
    public void Initialize(RaptureField raptureField)
    {
        _raptureField = raptureField;
    }

    /// <summary>
    /// 전투를 시작합니다.
    /// Caller: CombatSystem.InitializeAsync()
    /// </summary>
    public async Task StartBattleAsync(StageBattleGameData battleData)
    {
        _battleData = battleData;
        _currentPhaseIndex = 0;
        _totalKilledCount = 0;
        _accumulatedWeight = 0f;
        _aliveRaptures.Clear();

        Debug.Log($"[CombatWaveSystem] StartBattleAsync: BattleID {battleData.ID}");

        // 1. 첫 페이즈 시작
        StartPhase(_currentPhaseIndex);
    }

    // ==================== Private Methods ====================

    private void StartPhase(int index)
    {
        if (index >= _battleData.PhaseCount)
        {
            Debug.Log("[CombatWaveSystem] All phases complete!");
            OnAllPhasesComplete?.Invoke();
            return;
        }

        Debug.Log($"[CombatWaveSystem] StartPhase: {index}");
        _currentPhaseIndex = index;
        _currentPhaseKillCount = 0;

        int phaseId = _battleData.phaseIds[index];
        var phaseData = Managers.Data.Get<PhaseGameData>(phaseId);

        if (phaseData == null)
        {
            Debug.LogError($"[CombatWaveSystem] Phase data not found: {phaseId}");
            OnPhaseComplete?.Invoke(index); // 강제 완료 처리
            return;
        }

        _currentPhaseSpawnCount = phaseData.SpawnCount;
        SpawnPhaseRaptures(phaseData);
    }

    private async void SpawnPhaseRaptures(PhaseGameData phase)
    {
        // 시간순 정렬 (절대 시간 기준)
        var sortedSpawns = phase.spawns.OrderBy(x => x.spawnDelaySec).ToList();
        float lastSpawnTime = 0f;

        foreach (var entry in sortedSpawns)
        {
            // 이전 스폰 시간과의 차이만큼 대기
            float waitTime = entry.spawnDelaySec - lastSpawnTime;

            if (waitTime > 0)
            {
                await WaitGameTimeAsync(waitTime);
            }

            await SpawnRapture(entry);
            lastSpawnTime = entry.spawnDelaySec;
        }
    }

    /// <summary>
    /// 게임 시간 기준으로 대기합니다. (IsPaused 상태일 때는 시간이 흐르지 않음)
    /// </summary>
    private async Task WaitGameTimeAsync(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            await Task.Yield();

            // TimeManager의 일시정지 상태 확인
            if (Managers.Time != null && !Managers.Time.IsPaused)
            {
                elapsed += Time.deltaTime;
            }
        }
    }

    private async Task SpawnRapture(PhaseSpawnEntry entry)
    {
        // ==================== TEST CODE: Random Spawn Position ====================
        // TODO: 정식 스폰 시스템 구현 후 GetSpawnPosition()으로 교체
        // 1. 스폰 위치 결정 (랜덤)
        Vector3 spawnPos = _raptureField.GetRandomSpawnPosition(entry.spawnerId);
        // ==================== END TEST CODE ====================

        // 2. InstantiateAsync (내부적으로 풀링 사용 가능)
        // Managers.Resource.InstantiateAsync는 풀링 여부를 리소스 매니저/풀 매니저 정책에 따름
        GameObject go = await Managers.Resource.InstantiateAsync(RAPTURE_PREFAB_KEY, spawnPos);

        if (go == null)
        {
            Debug.LogError($"[CombatWaveSystem] Failed to instantiate rapture: {RAPTURE_PREFAB_KEY}");
            return;
        }

        // 3. CombatRapture 초기화
        var rapture = go.GetComponent<CombatRapture>();
        if (rapture == null)
        {
            Debug.LogError("[CombatWaveSystem] Rapture prefab missing CombatRapture component!");
            Managers.Resource.Destroy(go);
            return;
        }

        var gameData = Managers.Data.Get<RaptureGameData>(entry.raptureId);
        if (gameData == null)
        {
            Debug.LogError($"[CombatWaveSystem] Rapture data not found: {entry.raptureId}");
            // Fallback: 4001 (기본)
            gameData = Managers.Data.Get<RaptureGameData>(1);
        }

        // 구역 파싱 (SpawnerId "Near_Ground_1" -> Near)
        eRangeZone zone = ParseZoneFromSpawnerId(entry.spawnerId);

        rapture.Initialize(gameData, zone);
        rapture.OnDeath += OnRaptureKilled;

        _aliveRaptures.Add(rapture);

        // Zone에 등록
        var targetZone = _raptureField.GetZones(zone);
        if (targetZone != null && targetZone.Length > 0)
        {
            // 단순화: 첫 번째 해당 Zone에 추가 (실제로는 SpawnerId에 맞는 Zone을 찾아야 함)
            targetZone[0].AddRapture(rapture);
        }
    }

    private void OnRaptureKilled(CombatRapture rapture)
    {
        rapture.OnDeath -= OnRaptureKilled;
        _aliveRaptures.Remove(rapture);

        // Zone에서 제거
        var zones = _raptureField.GetZones(rapture.CurrentZone);
        if (zones != null && zones.Length > 0)
        {
            zones[0].RemoveRapture(rapture);
        }

        _totalKilledCount++;
        _currentPhaseKillCount++;

        // 트리거 시스템 알림용 이벤트 발행
        OnRaptureDied?.Invoke(rapture);

        // PoolManager로 반환 (ResourceManager를 통해)
        Managers.Resource.Destroy(rapture.gameObject);

        Debug.Log($"[CombatWaveSystem] Rapture Killed. Progress: {Progress * 100:F1}%");

        // 페이즈 완료 체크 (모든 랩쳐 처치 시)
        // 주의: 스폰이 아직 다 안 끝났을 수도 있으므로, _currentPhaseKillCount와 _currentPhaseSpawnCount 비교 권장
        // 하지만 기획상 "필드의 모든 랩쳐 처치"가 조건일 수 있음. 여기선 "모든 예정 랩쳐 처치"로 구현
        if (_currentPhaseKillCount >= _currentPhaseSpawnCount)
        {
            FinishPhase();
        }
    }

    private void FinishPhase()
    {
        Debug.Log($"[CombatWaveSystem] Phase {_currentPhaseIndex} Complete!");

        // 가중치 누적
        if (_currentPhaseIndex < _battleData.phaseProgressWeights.Length)
            _accumulatedWeight += _battleData.phaseProgressWeights[_currentPhaseIndex];

        OnPhaseComplete?.Invoke(_currentPhaseIndex);

        // 다음 페이즈
        StartPhase(_currentPhaseIndex + 1);
    }

    private eRangeZone ParseZoneFromSpawnerId(string spawnerId)
    {
        // "Near_Ground_1"
        if (string.IsNullOrEmpty(spawnerId)) return eRangeZone.Near;

        if (spawnerId.StartsWith("Near")) return eRangeZone.Near;
        if (spawnerId.StartsWith("Mid")) return eRangeZone.Mid;
        if (spawnerId.StartsWith("Far")) return eRangeZone.Far;

        return eRangeZone.Near;
    }
}
