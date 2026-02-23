using UnityEngine;

/// <summary>
/// 로켓 런처 (Rocket Launcher) 무기.
/// 차지를 모은 후 발사하며 착탄 지점 기준으로 범위 퍼짐(AoE) 데미지를 적용합니다.
/// </summary>
public class RLWeapon : ChargeWeaponBase
{
    public RLWeapon(WeaponData data) : base(data, eNikkeWeapon.RL) { }

    protected override void FireOnRelease(CombatNikke owner, long damage)
    {
        Vector2 screenPos = GetTargetScreenPosition(owner);
        Ray ray = _mainCamera.ScreenPointToRay(screenPos);

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 spawnPos = owner.transform.position + Vector3.up * 1.5f;
        Vector3 direction = (targetPoint - spawnPos).normalized;

        FireProjectileAsync(owner, damage, spawnPos, direction);
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
