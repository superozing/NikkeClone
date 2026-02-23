using UnityEngine;

/// <summary>
/// 기관단총 (Submachine Gun) 무기.
/// AR과 동일한 구조를 가지나 연사 속도 데이터가 더 빠름.
/// </summary>
public class SMGWeapon : DefaultWeaponBase
{
    public SMGWeapon(WeaponData data) : base(data, eNikkeWeapon.SMG) { }

    protected override void TryFire(CombatNikke owner, Vector3 targetWorldPos)
    {
        Vector3 mPos = owner.transform.position + Vector3.up * 1f;
        Vector3 direction = (targetWorldPos - mPos).normalized;

        if (Physics.Raycast(mPos, direction, out var hit, Mathf.Infinity, _layerMask))
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
