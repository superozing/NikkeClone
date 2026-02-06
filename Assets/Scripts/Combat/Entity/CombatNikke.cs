using UnityEngine;

/// <summary>
/// 전투용 니케 클래스입니다.
/// 씬에 미리 배치되어 있으며, 전략 패턴으로 데이터를 주입받습니다.
/// </summary>
public class CombatNikke : MonoBehaviour
{
    // ==================== Data ====================

    private NikkeGameData _gameData;
    private UserNikkeData _userData;

    // ==================== Properties ====================

    /// <summary>니케 ID</summary>
    public int NikkeId => _gameData?.id ?? -1;

    /// <summary>니케 이름</summary>
    public string NikkeName => _gameData?.name ?? "Unknown";

    /// <summary>현재 상태 (Phase 2에서 구현)</summary>
    public string CurrentStateName => "Idle"; // TODO: 상태 패턴 연동

    // ==================== Public Methods ====================

    /// <summary>
    /// 니케 데이터를 주입합니다.
    /// CombatScene.InitializeNikkes()에서 호출됩니다.
    /// </summary>
    public void Initialize(NikkeGameData gameData, UserNikkeData userData)
    {
        _gameData = gameData;
        _userData = userData;

        Debug.Log($"[CombatNikke] Initialized: {_gameData.name} (Lv.{_userData.level.Value})");

        // TODO Phase 2: 상태 머신 초기화
        // TODO Phase 2: 비주얼 설정 (스프라이트 등)
    }
}
