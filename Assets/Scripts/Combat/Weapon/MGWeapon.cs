using UnityEngine;

/// <summary>
/// 머신건 (Machine Gun) 무기.
/// AR과 동일한 메커니즘을 사용하며, 가장 빠른 연사 속도를 가집니다.
/// </summary>
public class MGWeapon : DefaultWeaponBase
{
    public MGWeapon(WeaponData data) : base(data, eNikkeWeapon.MG) { }

    protected override void TryFire(CombatNikke owner)
    {
        Vector2 screenPos = GetTargetScreenPosition(owner);
        if (PerformRaycast(screenPos, out var hit))
        {
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && !rapture.IsDead)
            {
                rapture.TakeDamage(CalculateDamage(owner, 1.0f));
            }
        }
        ConsumeAmmo(1);
    }
}
