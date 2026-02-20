using UnityEngine;

/// <summary>
/// 로켓 런처 (Rocket Launcher) 무기.
/// 차지를 모은 후 발사하며 착탄 지점 기준으로 범위 퍼짐(AoE) 데미지를 적용합니다.
/// </summary>
public class RLWeapon : ChargeWeaponBase
{
    private float _explosionRadius = 3.0f;

    public RLWeapon(WeaponData data) : base(data, eNikkeWeapon.RL) { }

    protected override void FireOnRelease(CombatNikke owner, long damage)
    {
        Vector2 screenPos = GetTargetScreenPosition(owner);
        if (PerformRaycast(screenPos, out var hit))
        {
            Collider[] colliders = Physics.OverlapSphere(hit.point, _explosionRadius, _layerMask);
            foreach (var col in colliders)
            {
                var rapture = col.GetComponent<CombatRapture>();
                if (rapture != null && !rapture.IsDead)
                {
                    rapture.TakeDamage(damage);
                }
            }
        }
    }
}
