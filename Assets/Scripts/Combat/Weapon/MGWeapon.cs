using UnityEngine;

/// <summary>
/// 머신건 (Machine Gun) 무기.
/// 발사 시 예열(Spin-up) 메커니즘을 가집니다.
/// 지속 사격 시 공격 속도가 점진적으로 증가합니다.
/// </summary>
public class MGWeapon : DefaultWeaponBase
{
    private float _minFireInterval = 0.3f; // 초기(가장 느린) 발사 간격
    private float _maxFireInterval;        // 최대 가속 시 발사 간격 (기본 _fireInterval)
    public override float GaugeChargePerHit => 0.005f;

    // 예열 게이지는 WeaponBase의 _chargeProgress 를 재사용합니다. (0.0 ~ 1.0)
    private float _spinUpPerShot = 0.1f;  // 발사당 차오르는 예열량
    private float _spinDownRate = 0.5f;    // 초당 식는 예열량

    public MGWeapon(WeaponData data) : base(data, eNikkeWeapon.MG)
    {
        // 원래 DefaultWeaponBase에서 설정된 _fireInterval을 최대 속도로 취급
        _maxFireInterval = _fireInterval;
        _fireInterval = _minFireInterval;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        // 예열 감소
        if (_chargeProgress.Value > 0f)
        {
            float timeSinceLastFire = Time.time - _lastFireTime;
            // 사격 중단 후 0.5초 경과 시점부터 예열 게이지 감소
            if (timeSinceLastFire > 0.5f)
            {
                _chargeProgress.Value = Mathf.Clamp01(_chargeProgress.Value - deltaTime * _spinDownRate);
            }
        }

        // 예열도에 따른 현재 발사 간격 갱신 (선형 보간)
        _fireInterval = Mathf.Lerp(_minFireInterval, _maxFireInterval, _chargeProgress.Value);
    }

    protected override void TryFire(CombatNikke owner, Vector3 targetWorldPos)
    {
        // 예열 단계에 따른 발사 지연 처리 (간이 구현)
        // 실제로는 Update에서 FireInterval을 조절하는 방식이 더 정확할 수 있습니다.
        Vector3 mPos = owner.transform.position + Vector3.up * 1f;
        Vector3 direction = (targetWorldPos - mPos).normalized;

        if (Physics.Raycast(mPos, direction, out var hit, Mathf.Infinity, _layerMask))
        {
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && !rapture.IsDead)
            {
                long damage = CalculateDamage(owner, 1.0f);
                rapture.TakeDamage(damage);
                NotifyHit(owner, damage);
            }
        }
        ConsumeAmmo(1);

        // 사격 시 예열 게이지 증가
        _chargeProgress.Value = Mathf.Clamp01(_chargeProgress.Value + _spinUpPerShot);
    }
}
