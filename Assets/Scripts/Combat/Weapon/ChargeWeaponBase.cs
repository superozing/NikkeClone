using UnityEngine;

/// <summary>
/// 차지형 무기 (SR, RL) 추상 클래스.
/// 공격 버튼을 홀드하여 차지를 모으고, 릴리스 시점에 누적된 차지에 비례한 배율로 1회 격발합니다.
/// </summary>
public abstract class ChargeWeaponBase : WeaponBase
{
    protected float _chargeTime;
    protected float _fullChargeMultiplier;
    
    protected float _currentCharge; // 0.0 ~ 1.0

    public ChargeWeaponBase(WeaponData data, eNikkeWeapon type) : base(data, type)
    {
        if (data != null)
        {
            _chargeTime = data.chargeTime > 0 ? data.chargeTime : 1.0f;
            _fullChargeMultiplier = data.fullChargeMultiplier > 0 ? data.fullChargeMultiplier : 2.5f;
        }
        else
        {
            _chargeTime = 1.0f;
            _fullChargeMultiplier = 2.5f;
        }
    }

    public override void Enter(CombatNikke owner)
    {
        _currentCharge = 0f;
    }

    public override void Update(CombatNikke owner)
    {
        if (!CanFire) return;

        // 차지 누적
        _currentCharge += Time.deltaTime / _chargeTime;
        _currentCharge = Mathf.Clamp01(_currentCharge);
    }

    public override void Exit(CombatNikke owner)
    {
        if (!CanFire) return;

        // 차지량에 따른 데미지 배율 적용 (기본 1.0배 ~ 풀차지)
        float currentMultiplier = Mathf.Lerp(1.0f, _fullChargeMultiplier, _currentCharge);
        long damage = CalculateDamage(owner, currentMultiplier);

        FireOnRelease(owner, damage);

        ConsumeAmmo(1); // 격발 후 탄약 1 감소
        _currentCharge = 0f;
    }

    /// <summary>
    /// 차지가 풀릴 때 격발 수행 (하위 클래스에서 디테일 구현)
    /// </summary>
    protected abstract void FireOnRelease(CombatNikke owner, long damage);
}
