using UnityEngine;

/// <summary>
/// 돌격소총 (Assault Rifle) 무기.
/// 기본형 데이터 제어로 연사 속도를 결정하며 한 발씩 Raycast 발사.
/// </summary>
public class ARWeapon : DefaultWeaponBase
{
    public ARWeapon(WeaponData data) : base(data, eNikkeWeapon.AR) { }
    public override float GaugeChargePerHit => 0.010f;

    protected override void TryFire(CombatNikke owner, Vector3 targetWorldPos)
    {
        // 임시로 owner 위치 대체
        Vector3 mPos = owner.transform.position + Vector3.up * 1f;

        Vector3 direction = (targetWorldPos - mPos).normalized;

        if (Physics.Raycast(mPos, direction, out var hit, Mathf.Infinity, _layerMask))
        {
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && !rapture.IsDead)
            {
                long damage = CalculateDamage(owner, 1.0f);
                rapture.TakeDamage(damage);
                NotifyHit(owner, damage, hit.point);
            }
        }
        ConsumeAmmo(1);
    }
}
