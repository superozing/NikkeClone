using UnityEngine;

/// <summary>
/// 돌격소총 (Assault Rifle) 무기.
/// 기본형 데이터 제어로 연사 속도를 결정하며 한 발씩 Raycast 발사.
/// </summary>
public class ARWeapon : DefaultWeaponBase
{
    public ARWeapon(WeaponData data) : base(data, eNikkeWeapon.AR) { }

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
