using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 수동 조준 전략. 마우스 화면 좌표를 즉시 반환합니다.
/// </summary>
public class ManualAimStrategy : IAimStrategy
{
    /// Caller: CombatNikke.UpdateAimPosition()
    public Vector2 GetAimScreenPosition(CombatNikke owner, Vector2 currentAimPos, float deltaTime)
    {
        return Mouse.current.position.ReadValue();
    }
}
