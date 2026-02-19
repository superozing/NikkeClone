using System.Threading.Tasks;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 전투 씬의 핵심 로직을 담당하는 오케스트레이터입니다.
/// `CombatScene`으로부터 위임받아 니케, 웨이브, UI, 게임 진행을 총괄합니다.
/// </summary>
public class CombatSystem : MonoBehaviour
{
    // ==================== Dependencies ====================
    [Header("Dependencies")]
    [SerializeField] private CombatNikke[] _nikkes; // 5 slot
    [SerializeField] private CombatWaveSystem _waveSystem;
    private UI_CombatHUD _combatHUD;

    // ==================== State ====================

    public float BurstGauge { get; set; } // Phase 9
    public int AliveNikkeCount { get; private set; }

    private bool _isCombatEnded;
    private float _timeLimitSec;

    private float _remainingTime;
    private int _lastRemainingSec = -1;



    private CombatHUDViewModel _hudViewModel;
    private StageGameData _stageGameData;

    // ==================== Events ====================

    public event System.Action<eCombatResult> OnCombatEnded;

    // ==================== Public Methods ====================

    /// <summary>
    /// 전투 시스템을 초기화하고 전투를 시작합니다.
    /// Caller: CombatScene.Init()
    /// </summary>
    public async Task InitializeAsync(int stageId, int squadId)
    {
        Debug.Log("[CombatSystem] InitializeAsync Start");

        _isCombatEnded = false;

        // 1. 데이터 로드
        var stageData = Managers.Data.Get<StageGameData>(stageId);
        if (stageData == null)
        {
            Debug.LogError($"[CombatSystem] StageGameData missing: {stageId}");
            return;
        }
        _stageGameData = stageData;

        var battleData = Managers.Data.Get<StageBattleGameData>(stageData.stageBattleDataId);
        if (battleData == null)
        {
            Debug.LogError($"[CombatSystem] StageBattleGameData missing: {stageData.stageBattleDataId}");
            return;
        }

        _timeLimitSec = battleData.timeLimitSec > 0 ? battleData.timeLimitSec : 180;
        _remainingTime = _timeLimitSec;

        // 2. WaveSystem 초기화
        if (_waveSystem == null)
            _waveSystem = FindFirstObjectByType<CombatWaveSystem>();

        if (_waveSystem != null)
        {
            // RaptureField 처리 (WaveSystem 내부에서 필요시 찾음, 여기선 생략 가능하지만 명시적 초기화가 좋음)
            // CombatWaveSystem 리팩토링 시 Initialize에 의존성 주입 고려
            // 현재는 기존 WaveSystem 구조 유지하며 이름만 변경 예정
            _waveSystem.OnAllPhasesComplete += OnAllPhasesComplete;
            // _waveSystem.StartBattleAsync 호출은 아래에서
        }

        // 3. UI, Nikke 초기화
        await InitializeNikkesAsync(squadId);
        await InitializeHUDAsync();

        // 4. 전투 시작 (웨이브)
        if (_waveSystem != null)
        {
            await _waveSystem.StartBattleAsync(battleData);
        }

        Debug.Log("[CombatSystem] Initialization Complete. Battle Started.");
        _isInitialized = true;
    }



    /// <summary>
    /// 니케 사망 시 호출
    /// Caller: CombatNikke.Die()
    /// </summary>
    public void OnNikkeDied(CombatNikke nikke)
    {
        AliveNikkeCount--;
        Debug.Log($"[CombatSystem] Nikke Died: {nikke.NikkeName}. Alive: {AliveNikkeCount}");

        if (AliveNikkeCount <= 0)
        {
            EndCombat(eCombatResult.Defeat);
            return;
        }

        // 현재 조작 중인 니케가 죽었다면? -> CombatSystem이 판단해서 Switch 요청?
        // CombatNikke가 스스로 판단하기 어려움 (다른 생존 니케를 모르므로)
        // 따라서 여기서 다음 니케를 찾아 NotifySwitched(또는 특정 니케 Activate) 호출

        // TODO: 사망한 니케가 현재 조작중이었는지 확인 필요
        // 하지만 CombatSystem은 _activeNikkeIndex를 굳이 들고 있을 필요 없게 설계함 (느슨한 결합)
        // 그래도 편의상 들고 있거나, 아니면 Nikke 상태를 순회해야 함.

        // 순회해서 다음 조작 니케 찾기
        // 만약 방금 죽은 니케가 Manual이었다면, 얘가 죽으면서 Manual 상태가 꺼졌음.
        // 누군가는 Manual이 되어야 함.

        // 간단한 로직: 조작 중인 니케가 없으면(모두 Auto/Dead) 0번부터 순회해서 첫 생존자를 Manual로 만듦
        // 이를 위해 매 프레임 체크하기보다, 사망 이벤트 시점에 체크

        for (int i = 0; i < _nikkes.Length; i++)
        {
            if (_nikkes[i] != null && !_nikkes[i].IsDead)
            {
                // Phase 6.1 Fix: Use ForceActivate
                _nikkes[i].ForceActivate();
                return;
            }
        }
    }

    /// <summary>
    /// 전투 종료 및 정리
    /// Caller: CombatScene.Clear()
    /// </summary>
    public void Cleanup()
    {
        if (_waveSystem != null)
        {
            _waveSystem.OnAllPhasesComplete -= OnAllPhasesComplete;
        }

        // 이벤트 해제 등
    }

    // ==================== Private Methods ====================

    private bool _isInitialized = false;

    private void Update()
    {
        if (!_isInitialized || _isCombatEnded) return;

        // 타이머 업데이트 (Phase 6.1 Optimization: 초 단위 변경 시에만 갱신)
        _remainingTime -= Time.deltaTime;
        int currentSec = Mathf.FloorToInt(Mathf.Max(0, _remainingTime));

        if (currentSec != _lastRemainingSec)
        {
            _lastRemainingSec = currentSec;

            int minutes = currentSec / 60;
            int seconds = currentSec % 60;

            if (_hudViewModel != null)
            {
                _hudViewModel.TimeText.Value = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        // Phase 6.1 Fix: HUD Progress Update
        if (_combatHUD != null && _waveSystem != null)
        {
            _combatHUD.UpdateProgress(_waveSystem.Progress);
        }

        if (_remainingTime <= 0)
        {
            EndCombat(eCombatResult.Timeout);
        }
    }

    private async Task InitializeNikkesAsync(int squadId)
    {
        AliveNikkeCount = 0;
        var squadData = Managers.Data.UserData.Squads[squadId];
        if (squadData == null || squadData.slot == null)
        {
            Debug.LogError($"[CombatSystem] Invalid SquadData for ID: {squadId}");
            return;
        }

        if (_nikkes == null || _nikkes.Length == 0)
        {
            Debug.LogError("[CombatSystem] No CombatNikke found in scene.");
            return;
        }

        for (int i = 0; i < _nikkes.Length && i < squadData.slot.Count; i++)
        {
            if (_nikkes[i] == null) continue;

            int nikkeId = squadData.slot[i];
            var gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
            var userData = Managers.Data.UserData.Nikkes[nikkeId];

            await _nikkes[i].InitializeAsync(gameData, userData, i, this);

            AliveNikkeCount++;
        }

        // 초기 조작 니케 설정 (2번이 기본)
        int defaultIndex = 2;
        if (defaultIndex < _nikkes.Length && _nikkes[defaultIndex] != null)
        {
            _nikkes[defaultIndex].ForceActivate();
        }
        else if (_nikkes.Length > 0 && _nikkes[0] != null)
        {
            _nikkes[0].ForceActivate();
        }
    }

    private async Task InitializeHUDAsync()
    {
        if (_combatHUD == null)
        {
            // ViewModel 생성 및 데이터 주입
            _hudViewModel = new CombatHUDViewModel(_nikkes);

            // UI 매니저를 통해 HUD 생성 (ViewModel 전달)
            _combatHUD = await Managers.UI.ShowAsync<UI_CombatHUD>(_hudViewModel);

            if (_combatHUD == null)
            {
                Debug.LogError("[CombatSystem] Failed to load UI_CombatHUD");
                return;
            }
        }
    }

    private void OnAllPhasesComplete()
    {
        EndCombat(eCombatResult.Victory);
    }

    private void EndCombat(eCombatResult result)
    {
        if (_isCombatEnded) return;
        _isCombatEnded = true;

        Debug.Log($"[CombatSystem] EndCombat: {result}");
        OnCombatEnded?.Invoke(result); // CombatScene에게 알림 (팝업 등 처리는 CombatScene이 할 수도, System이 할 수도 있음. 설계상 System이 주도)

        // 팝업 표시
        if (result == eCombatResult.Victory)
        {
            var viewModel = new CombatResultVictoryPopupViewModel(_stageGameData?.rewards ?? new List<RewardData>());
            _ = Managers.UI.ShowAsync<UI_CombatResultVictoryPopup>(viewModel);
        }
        else
        {
            var viewModel = new CombatResultDefeatPopupViewModel();
            _ = Managers.UI.ShowAsync<UI_CombatResultDefeatPopup>(viewModel);
        }
    }

    /// <summary>
    /// 디버그용 강제 전투 종료
    /// </summary>
    public void ForceEndCombat(eCombatResult result)
    {
        Debug.Log($"[CombatSystem] ForceEndCombat: {result}");
        EndCombat(result);
    }
}
