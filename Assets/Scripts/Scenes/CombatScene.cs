using System.Collections.Generic;
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

    void IScene.Init()
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
        InitializeNikkes();

        // 3. HUD 초기화
        InitializeHUD();

        // TODO Phase 4: 웨이브 시작
    }

    private void InitializeNikkes()
    {
        var squadData = Managers.Data.UserData.Squads[_squadId];

        for (int i = 0; i < _nikkes.Length && i < squadData.slot.Count; i++)
        {
            int nikkeId = squadData.slot[i];

            var gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
            var userData = Managers.Data.UserData.Nikkes[nikkeId];

            // 전략 패턴: 니케 오브젝트에 데이터 주입
            _nikkes[i].Initialize(gameData, userData, i);
        }

        Debug.Log($"[CombatScene] Initialized {_nikkes.Length} nikkes");
    }

    private async void InitializeHUD()
    {
        _combatHUDViewModel = new CombatHUDViewModel(_nikkes);
        _combatHUDViewModel.AddRef();

        _combatHUD = await Managers.UI.ShowAsync<UI_CombatHUD>(_combatHUDViewModel);
    }

    void IScene.Clear()
    {
        Debug.Log("[CombatScene] Clear");

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
