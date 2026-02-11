using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 전투 씬의 진입점입니다.
/// IScene 인터페이스를 구현하여 SceneManagerEx와 연동합니다.
/// </summary>
public class CombatScene : MonoBehaviour, IScene
{
    // ==================== IScene Implementation ====================

    eSceneType IScene.SceneType => eSceneType.CombatScene;

    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "StageGameData.json",
        "StageBattleGameData.json",
        "PhaseGameData.json",
        "RaptureGameData.json",
        "MissionGameData.json",
    };

    // ==================== SerializeFields ====================

    [SerializeField] private CombatNikke[] _nikkes;  // 5개, Inspector에서 할당
    [SerializeField] private Camera _mainCamera;     // Phase 3: 레이캐스트용 카메라
    [SerializeField] private LayerMask _raptureLayer;// Phase 3: 랩쳐 레이어

    [Header("Phase 4: Wave System")]
    [SerializeField] private WaveSystem _waveManager;
    [SerializeField] private RaptureField _raptureField; // Inspector 할당용 (WaveSystem가 사용)

    // ==================== Runtime ====================

    private UI_CombatHUD _combatHUD;
    private CombatHUDViewModel _combatHUDViewModel;

    private int _stageId;
    private int _squadId;

    // ==================== Unity Lifecycle ====================

    private void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    private void Update()
    {
        // Phase 3: 입력 처리
        // 공격 시작 (눌렀을 때)
        if (Input.GetMouseButtonDown(0))
        {
            HandleInput(true);
        }
        // 공격 중지 (뗐을 때)
        else if (Input.GetMouseButtonUp(0))
        {
            HandleInput(false);
        }

        // Phase 4: UI 업데이트
        if (_waveManager != null && _combatHUD != null)
        {
            _combatHUD.UpdateProgress(_waveManager.Progress);
        }
    }

    async void IScene.Init()
    {
        // 1. 유저 데이터에서 전투 파라미터 읽기
        var combatData = Managers.Data.UserData.Combat;
        if (combatData == null)
        {
            Debug.LogError("[CombatScene] Combat data is null! 캠페인에서 전투를 시작해주세요.");
            return;
        }

        _stageId = combatData.stageId;
        _squadId = combatData.squadId;
        Debug.Log($"[CombatScene] Init - Stage: {_stageId}, Squad: {_squadId}");

        // 2. 니케에 데이터 주입
        await InitializeNikkesAsync();

        // 3. HUD 초기화
        InitializeHUD();

        // Phase 4: 웨이브 시스템 동적 생성 및 시작
        if (_waveManager == null)
        {
            // WaveSystem이 없으면 동적 생성
            GameObject wmGo = new GameObject("WaveSystem");
            _waveManager = wmGo.AddComponent<WaveSystem>();
            Debug.Log("[CombatScene] WaveSystem dynamically created.");
        }

        if (_raptureField == null)
        {
            // RaptureField가 없으면 찾거나 생성
            _raptureField = FindFirstObjectByType<RaptureField>();
            if (_raptureField == null)
            {
                GameObject rfGo = new GameObject("RaptureField");
                _raptureField = rfGo.AddComponent<RaptureField>();
                // 주의: RaptureField는 Zone 설정 데이터가 필요하므로, 
                // 빈 오브젝트로 생성되면 정상 동작하지 않을 수 있음.
                // 프리팹을 로드하는 방식이 안전함 (추후 개선 포인트)
                Debug.LogWarning("[CombatScene] RaptureField dynamically created (Empty!). Zone data might be missing.");
            }
        }

        // WaveSystem 초기화 (RaptureField 주입)
        _waveManager.Initialize(_raptureField);
        _waveManager.OnAllPhasesComplete += OnAllPhasesComplete;


        // 필요한 데이터 로드
        var stageData = Managers.Data.Get<StageGameData>(_stageId);
        if (stageData != null)
        {
            var battleData = Managers.Data.Get<StageBattleGameData>(stageData.stageBattleDataId);
            if (battleData != null)
            {
                await _waveManager.StartBattleAsync(battleData);
            }
            else
            {
                Debug.LogError($"[CombatScene] StageBattleGameData not found for ID: {stageData.stageBattleDataId}");
            }
        }
        else
        {
            Debug.LogError($"[CombatScene] StageGameData not found for ID: {_stageId}");
        }

        /* Phase 3 테스트 코드 제거
        var raptures = FindObjectsByType<CombatRapture>(FindObjectsSortMode.None);
        foreach (var rapture in raptures)
        {
            if (rapture.RaptureId == -1) // -1: Uninitialized
            {
                var raptureData = Managers.Data.Get<RaptureGameData>(1);
                if (raptureData != null)
                {
                    rapture.Initialize(raptureData, eRangeZone.Near);
                }
            }
            rapture.OnDeath += OnRaptureKilled;
        }
        */
    }

    private void HandleInput(bool isDown)
    {
        // 0번 니케가 발사 가능한지 체크 (Phase 3: 단일 니케 조작 가정)
        // 실제 게임에서는 선택된 니케가 발사하지만, 지금은 0번 고정
        if (_nikkes == null || _nikkes.Length == 0 || _nikkes[0] == null) return;

        var nikke = _nikkes[0];

        if (isDown)
        {
            // 눌렀을 때: Attack 상태 전환 + 발사 시도
            nikke.StartAttack();

            if (!nikke.CanFire)
            {
                // 재장전 중이거나 사망 시
                Debug.Log("[CombatScene] Cannot fire - reloading or dead");
                return;
            }

            // 발사 로직 (기존 HandleClick 내용)
            // 마우스 위치에서 레이캐스트
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _raptureLayer))
            {
                var rapture = hit.collider.GetComponent<CombatRapture>();
                if (rapture != null && !rapture.IsDead)
                {
                    OnRaptureHit(rapture);
                }
            }
            else
            {
                // 빗나감 - 탄약만 소비
                nikke.ConsumeAmmo();
                Debug.Log("[CombatScene] Shot missed");
            }
        }
        else
        {
            // 뗐을 때: Reload/Cover 전환
            nikke.StopAttack();
        }
    }

    private void OnRaptureHit(CombatRapture rapture)
    {
        if (_nikkes == null || _nikkes.Length == 0) return;

        // 0번 니케가 발사
        _nikkes[0].Fire(rapture);
    }

    private void OnRaptureKilled(CombatRapture rapture)
    {
        Debug.Log($"[CombatScene] Rapture Killed: {rapture.RaptureName}");
    }

    private async Task InitializeNikkesAsync()
    {
        var squadData = Managers.Data.UserData.Squads[_squadId];

        for (int i = 0; i < _nikkes.Length && i < squadData.slot.Count; i++)
        {
            int nikkeId = squadData.slot[i];

            var gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
            var userData = Managers.Data.UserData.Nikkes[nikkeId];

            // 전략 패턴: 니케 오브젝트에 데이터 주입
            await _nikkes[i].InitializeAsync(gameData, userData, i);
        }

        Debug.Log($"[CombatScene] Initialized {_nikkes.Length} nikkes");
    }

    private async void InitializeHUD()
    {
        _combatHUDViewModel = new CombatHUDViewModel(_nikkes);
        _combatHUDViewModel.AddRef();

        _combatHUD = await Managers.UI.ShowAsync<UI_CombatHUD>(_combatHUDViewModel);
    }

    private void OnAllPhasesComplete()
    {
        Debug.Log("==========================================");
        Debug.Log("[CombatScene] VICTORY! All phases complete.");
        Debug.Log("==========================================");
    }

    void IScene.Clear()
    {
        Debug.Log("[CombatScene] Clear");

        // 이벤트 해제
        if (_waveManager != null)
        {
            _waveManager.OnAllPhasesComplete -= OnAllPhasesComplete;
        }

        // 전투 데이터 초기화
        Managers.Data.UserData.Combat = null;

        // HUD 정리
        if (_combatHUD != null)
        {
            Managers.UI.Close(_combatHUD);
            _combatHUD = null;
        }

        if (_combatHUDViewModel != null)
        {
            _combatHUDViewModel.Release();
            _combatHUDViewModel = null;
        }
    }
}
