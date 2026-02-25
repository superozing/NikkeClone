using UnityEngine;

/// <summary>
/// "조준 스크린 좌표"를 결정하는 전략 인터페이스.
/// 사격 로직을 포함하지 않습니다. 좌표 산출만 담당합니다.
/// State와 무관하게 CombatNikke.Update()에서 매 프레임 호출됩니다.
/// </summary>
public interface IAimStrategy
{
    /// <summary>
    /// 이번 프레임의 조준 스크린 좌표를 반환합니다.
    /// </summary>
    /// <param name="owner">본체 니케</param>
    /// <param name="currentAimPos">현재 조준 스크린 좌표 (보간 기준점)</param>
    /// <param name="deltaTime">프레임 간격</param>
    /// <returns>이번 프레임의 조준 스크린 좌표</returns>
    /// Caller: CombatNikke.UpdateAimPosition()
    Vector2 GetAimScreenPosition(CombatNikke owner, Vector2 currentAimPos, float deltaTime);
}
