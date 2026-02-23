using UnityEngine;

/// <summary>
/// 샷건 (Shotgun) 무기.
/// 1회 격발 시 여러 발의 산탄(Pellet)이 퍼져서 발사됩니다.
/// </summary>
public class SGWeapon : DefaultWeaponBase
{
    private int _pelletCount = 6;
    private float _spreadAngle = 5f;

    public SGWeapon(WeaponData data) : base(data, eNikkeWeapon.SG) { }

    public override eRangeZone PreferredZone => eRangeZone.Near;

    protected override void TryFire(CombatNikke owner, Vector3 targetWorldPos)
    {
        // 샷건은 각 펠릿당 데미지를 분산시킵니다.
        long perPelletDamage = CalculateDamage(owner, 1.0f) / _pelletCount;

        Vector3 mPos = owner.transform.position + Vector3.up * 1f;
        Vector3 baseDirection = (targetWorldPos - mPos).normalized;

        for (int i = 0; i < _pelletCount; i++)
        {
            Vector2 spread = Random.insideUnitCircle * _spreadAngle;
            Vector3 spreadDir = Quaternion.Euler(spread.y, spread.x, 0) * baseDirection;

            if (Physics.Raycast(mPos, spreadDir, out var hit, Mathf.Infinity, _layerMask))
            {
                var rapture = hit.collider.GetComponent<CombatRapture>();
                if (rapture != null && !rapture.IsDead)
                {
                    rapture.TakeDamage(perPelletDamage);
                }
            }
        }

        ConsumeAmmo(1);
    }
}
