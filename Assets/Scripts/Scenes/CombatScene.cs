using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 씬의 진입점입니다.
/// Phase 6: God Class 역할을 버리고 CombatSystem을 초기화/중계하는 역할로 축소되었습니다.
/// </summary>
public class CombatScene : MonoBehaviour, IScene
{
    // ==================== IScene Implementation ====================

    eSceneType IScene.SceneType => eSceneType.CombatScene;
    public string DefaultActionMapKey => "Combat";

    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "StageGameData.json",
        "StageBattleGameData.json",
        "PhaseGameData.json",
        "RaptureGameData.json",
        "MissionGameData.json",
        "ItemGameData.json"
    };

    // ==================== SerializeFields ====================

    [SerializeField] private CombatSystem _combatSystem;

    // ==================== Events ====================

    // 외부에서 구독하는 이벤트
    public event System.Action<eCombatResult> OnCombatEnded;

    // ==================== Unity Lifecycle ====================

    // Awake(), Update()도 제거 가능 (필요 시 복구)
    private void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
    }

    // ==================== IScene Logic ====================

    async void IScene.Init()
    {
        // 1. 유저 데이터에서 전투 파라미터 읽기
        var combatData = Managers.Data.UserData.Combat;
        if (combatData == null)
        {
            Debug.LogError("[CombatScene] Combat data is null! 캠페인에서 전투를 시작해주세요.");
            return;
        }

        Debug.Log($"[CombatScene] Init - Stage: {combatData.stageId}, Squad: {combatData.squadId}");

        // 2. CombatSystem 초기화
        if (_combatSystem == null)
        {
            _combatSystem = FindFirstObjectByType<CombatSystem>();
            if (_combatSystem == null)
            {
                // 동적 생성
                GameObject go = new GameObject("CombatSystem");
                _combatSystem = go.AddComponent<CombatSystem>();
            }
        }

        // 이벤트 연결
        _combatSystem.OnCombatEnded += NotifyCombatEnded;

        // 3. 전투 초기화 및 시작 위임
        await _combatSystem.InitializeAsync(combatData.stageId, combatData.squadId);

        // Phase 6: Debug Keys (System이 아니라 Scene에서 바인딩하는 게 나을 수 있음 - 테스트용)
        BindDebugKeys();
    }

    void IScene.Clear()
    {
        Debug.Log("[CombatScene] Clear");

        // 이벤트 해제
        if (_combatSystem != null)
        {
            _combatSystem.OnCombatEnded -= NotifyCombatEnded;
            _combatSystem.Cleanup();
        }

        // Input 비우기
        Managers.Input.Clear();

        // 전투 데이터 초기화
        Managers.Data.UserData.Combat = null;
    }

    private void NotifyCombatEnded(eCombatResult result)
    {
        OnCombatEnded?.Invoke(result);
    }

    private void BindDebugKeys()
    {
        // Phase 6: Debug Keys
        Managers.Input.BindAction("DebugWin", _ => _combatSystem?.ForceEndCombat(eCombatResult.Victory));
        Managers.Input.BindAction("DebugLose", _ => _combatSystem?.ForceEndCombat(eCombatResult.Defeat));
    }
}
