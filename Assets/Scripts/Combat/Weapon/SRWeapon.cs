using UnityEngine;

/// <summary>
/// 스나이퍼 라이플 (Sniper Rifle) 무기.
/// 차지를 모은 후 화면의 지정된 좌표로 강력한 1발을 레이캐스트로 쏩니다.
/// </summary>
public class SRWeapon : ChargeWeaponBase
{
    public SRWeapon(WeaponData data) : base(data, eNikkeWeapon.SR) { }
    public override float GaugeChargePerHit => 0.100f;

    public override eRangeZone PreferredZone => eRangeZone.Far;

    protected override void FireOnRelease(CombatNikke owner, long damage, Vector3 targetWorldPos)
    {
        Vector3 mPos = owner.transform.position + Vector3.up * 1f;
        Vector3 direction = (targetWorldPos - mPos).normalized;

        if (Physics.Raycast(mPos, direction, out var hit, Mathf.Infinity, _layerMask))
        {
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && !rapture.IsDead)
            {
                rapture.TakeDamage(damage);
                NotifyHit(owner);
            }
        }
    }
}
