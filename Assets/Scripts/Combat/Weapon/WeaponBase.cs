using UnityEngine;

/// <summary>
/// 무기 시스템의 최상위 추상 클래스.
/// 탄약 관리, 재장전, 데미지 계산, 레이캐스트 보조 함수 등 공통 로직을 처리합니다.
/// </summary>
public abstract class WeaponBase : IWeapon
{
    protected Camera _mainCamera;
    protected int _layerMask;

    // Implements Section 2.1: IWeapon & WeaponBase 리팩토링
    protected ReactiveProperty<int> _currentAmmo = new ReactiveProperty<int>(0);
    protected ReactiveProperty<float> _chargeProgress = new ReactiveProperty<float>(0f);

    protected int _maxAmmo;
    protected float _reloadTime;
    protected float _damagePercent;
    protected eNikkeWeapon _weaponType;

    public eNikkeWeapon WeaponType => _weaponType;
    public bool CanFire => _currentAmmo.Value > 0;

    // Implements Section 2.1: IWeapon & WeaponBase 리팩토링
    public ReactiveProperty<int> CurrentAmmo => _currentAmmo;
    public ReactiveProperty<float> ChargeProgress => _chargeProgress;

    public int MaxAmmo => _maxAmmo;
    public float ReloadTime => _reloadTime;
    public float DamagePercent => _damagePercent;

    // 일반 무기는 배율 1.0 고정
    public virtual float FullChargeMultiplier => 1.0f;

    // 무기 적정 사거리. 파생 클래스에서 override
    public virtual eRangeZone PreferredZone => eRangeZone.Mid;

    /// <summary>
    /// 타겟이 적정 사거리 내에 있는지 여부를 나타냅니다.
    /// Derived Classes(파생 클래스)나 State(상태)에서 이 값을 업데이트하여 UI에 알립니다.
    /// </summary>
    public ReactiveProperty<bool> IsInPreferredZone { get; } = new ReactiveProperty<bool>(false);

    public ReactiveProperty<NikkeClone.Utils.eNikkeCombatMode> CombatMode { get; } = new ReactiveProperty<NikkeClone.Utils.eNikkeCombatMode>(NikkeClone.Utils.eNikkeCombatMode.Auto);
    public ReactiveProperty<Vector2> AutoTargetScreenPosition { get; } = new ReactiveProperty<Vector2>(Vector2.zero);
    public ReactiveProperty<Vector2> CurrentAimScreenPosition { get; } = new ReactiveProperty<Vector2>(Vector2.zero);

    public virtual float GetRangeAdvantageMultiplier(eRangeZone targetZone)
    {
        return targetZone == PreferredZone ? 1.2f : 1.0f;
    }

    public virtual bool IsPreferredZone(eRangeZone targetZone)
    {
        return targetZone == PreferredZone;
    }

    public WeaponBase(WeaponData data, eNikkeWeapon type)
    {
        _weaponType = type;
        if (data != null)
        {
            _maxAmmo = data.maxAmmo;
            _reloadTime = data.reloadTime;
            _damagePercent = data.damagePercent;
        }
        else
        {
            _maxAmmo = 30;
            _reloadTime = 1.5f;
            _damagePercent = 100f;
        }

        _currentAmmo.Value = _maxAmmo;
        _chargeProgress.Value = 0f;

        _mainCamera = Camera.main;
        // 적(CombatRapture)과 지형 장애물(CombatObstacle)을 모두 검출하기 위한 레이어 마스크
        _layerMask = LayerMask.GetMask("CombatRapture", "CombatObstacle"); // TODO: "CombatObstacle" 레이어 추가해야 해요.
    }

    public virtual void Enter(CombatNikke owner) { }

    public virtual void Update(CombatNikke owner, Vector3 targetWorldPos) { }

    public virtual void Exit(CombatNikke owner, bool isCancel = false) { }

    public virtual void Tick(float deltaTime) { }

    public void Reload()
    {
        _currentAmmo.Value = _maxAmmo;
    }

    public void ConsumeAmmo(int amount)
    {
        _currentAmmo.Value = Mathf.Max(0, _currentAmmo.Value - amount);
    }





    /// <summary>
    /// 데미지 계산 유틸리티
    /// </summary>
    protected long CalculateDamage(CombatNikke owner, float multiplier)
    {
        return CalculateDamage(owner, multiplier, 1.0f);
    }

    // [추가] 어드밴티지 배율 포함 오버로드
    protected long CalculateDamage(CombatNikke owner, float multiplier, float rangeAdvantage)
    {
        // owner.Status.attack가 기본 공격력이지만, 버프 등이 적용된 최종 공격력을 고려할 수 있음
        // 지금은 BaseStatus 기반으로 적용
        float baseAttack = owner.Status.attack;
        float finalDamage = baseAttack * (_damagePercent / 100f) * multiplier * rangeAdvantage;
        return (long)finalDamage;
    }
}
