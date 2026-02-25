using UnityEngine;

/// <summary>
/// 자동 조준 전략. 우선순위 타겟의 스크린 좌표를 향해 매 프레임 부드럽게 이동합니다.
/// </summary>
public class AutoAimStrategy : IAimStrategy
{
    /// <summary>
    /// 조준점 이동 속도 (픽셀/초). 전역 상수.
    /// </summary>
    private const float AIM_SPEED = 800f;

    /// Caller: CombatNikke.UpdateAimPosition()
    public Vector2 GetAimScreenPosition(CombatNikke owner, Vector2 currentAimPos, float deltaTime)
    {
        var target = owner.TargetingSystem?.GetTarget(owner.Weapon.PreferredZone);
        if (target == null || target.IsDead) return currentAimPos;

        Vector3 worldPos = target.transform.position;
        Vector2 targetScreenPos = owner.CachedCamera.WorldToScreenPoint(worldPos);

        return Vector2.MoveTowards(currentAimPos, targetScreenPos, AIM_SPEED * deltaTime);
    }
}
