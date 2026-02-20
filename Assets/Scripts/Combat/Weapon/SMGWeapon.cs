using UnityEngine;

/// <summary>
/// 기관단총 (Submachine Gun) 무기.
/// AR과 동일한 구조를 가지나 연사 속도 데이터가 더 빠름.
/// </summary>
public class SMGWeapon : DefaultWeaponBase
{
    public SMGWeapon(WeaponData data) : base(data, eNikkeWeapon.SMG) { }

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
