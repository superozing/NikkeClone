using UnityEngine;

/// <summary>
/// RL 등 투사체 기반 무기의 실제 탄환 역할 수행.
/// 지정된 방향으로 날아가며 충돌 시 범위(AoE) 데미지 계산.
/// Implements Section 2.1: CombatProjectile.cs
/// </summary>
public class CombatProjectile : MonoBehaviour
{
    private CombatNikke _owner;
    private long _damage;
    private float _speed;
    private Vector3 _direction;
    private bool _isInitialized = false;

    /// <summary>
    /// 투사체 발사 초기화
    /// Caller: RLWeapon.FireProjectileAsync()
    /// Intent: 데미지, 방향, 속도를 받아 투사체 이동 방향을 결정하고 초기화합니다.
    /// </summary>
    public void Initialize(CombatNikke owner, long damage, float speed, Vector3 direction)
    {
        _owner = owner;
        _damage = damage;
        _speed = speed;
        _direction = direction.normalized;
        _isInitialized = true;

        // 5초 후 자동 소멸 (화면 밖으로 나갈 경우 대비)
        Invoke(nameof(ReturnToPool), 5f);
    }

    private void ReturnToPool()
    {
        if (gameObject.activeInHierarchy)
        {
            Managers.Resource.Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!_isInitialized) return;

        transform.position += _direction * _speed * Time.deltaTime;
    }

    /// <summary>
    /// 충돌 판정 및 폭발 처리
    /// Intent: Physics 충돌 이벤트에 의해 폭발 반경만큼 데미지를 줍니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (!_isInitialized) return;

        // 투사체의 콜라이더 직접 충돌
        var rapture = other.GetComponent<CombatRapture>();
        if (rapture != null && !rapture.IsDead)
        {
            rapture.TakeDamage(_damage);

            // 버스트 게이지 충전을 위해 무기에 적중 알림
            if (_owner != null && _owner.Weapon is WeaponBase weaponBase)
            {
                weaponBase.NotifyHit(_owner);
            }
        }

        // 지형지물이거나 적이거나 충돌하면 투사체 반환
        // TODO: 투사체 파괴 효과(이프펙트) 추가 가능
        CancelInvoke(nameof(ReturnToPool));
        ReturnToPool();
    }
}
