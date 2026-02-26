using UnityEngine;

/// <summary>
/// 로켓 런처 (Rocket Launcher) 무기.
/// 차지를 모은 후 발사하며 착탄 지점 기준으로 범위 퍼짐(AoE) 데미지를 적용합니다.
/// </summary>
public class RLWeapon : ChargeWeaponBase
{
    public RLWeapon(WeaponData data) : base(data, eNikkeWeapon.RL) { }
    public override float GaugeChargePerHit => 0.15f;

    public override eRangeZone PreferredZone => eRangeZone.Near;
    public override bool IsPreferredZone(eRangeZone targetZone) => false;

    protected override void FireOnRelease(CombatNikke owner, long damage, Vector3 targetWorldPos)
    {
        Vector3 mPos = owner.transform.position + Vector3.up * 1.5f;
        Vector3 direction = (targetWorldPos - mPos).normalized;

        // RLWeapon은 투사체를 날립니다. targetWorldPos를 향해 발사.
        // 추가 보정이 필요하다면 여기서 수행 (예: 사거리 끝까지 날리기)

        FireProjectileAsync(owner, damage, mPos, direction);
    }

    private async void FireProjectileAsync(CombatNikke owner, long damage, Vector3 spawnPos, Vector3 direction)
    {
        // TODO: Addressables에 "RL_Projectile" 키 등록 필요
        GameObject projObj = await Managers.Resource.InstantiateAsync("Prefabs/Combat/RL_Projectile", spawnPos, Quaternion.LookRotation(direction));

        if (projObj != null)
        {
            var projectile = projObj.GetComponent<CombatProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(owner, damage, 50f, direction);
            }
        }
        else
        {
            Debug.LogError("[RLWeapon] RL_Projectile Addressable load failed.");
        }
    }
}
