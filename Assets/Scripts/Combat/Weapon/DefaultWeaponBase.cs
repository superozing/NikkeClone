using UnityEngine;

/// <summary>
/// 기본형 무기 (AR, SMG, MG, SG) 추상 클래스.
/// 공격 버튼을 누르고 있는 동안 지정된 인터벌(_fireInterval)마다 반복 발사합니다.
/// </summary>
public abstract class DefaultWeaponBase : WeaponBase
{
    protected float _fireInterval;
    protected float _lastFireTime;

    public DefaultWeaponBase(WeaponData data, eNikkeWeapon type) : base(data, type)
    {
        if (data != null && data.fireRate > 0)
        {
            _fireInterval = 1f / data.fireRate;
        }
        else
        {
            _fireInterval = 0.1f; // 기본 10발/초
        }
    }

    public override void Enter(CombatNikke owner)
    {
        // 진입 즉시 발사 가능하도록 쿨타임 초기화
        _lastFireTime = -_fireInterval;
    }

    protected override void Update(CombatNikke owner, Vector3 targetWorldPos)
    {
        if (!CanFire) return;

        if (Time.time - _lastFireTime >= _fireInterval)
        {
            _lastFireTime = Time.time;
            TryFire(owner, targetWorldPos);
        }
    }

    public override void Exit(CombatNikke owner, bool isCancel = false)
    {
        // 기본형은 버튼 해제 시 아무 동작 안 함
    }

    /// <summary>
    /// 일반형 무기 전투 처리.
    /// Auto 모드에서는 CombatRapture 타겟에만 사격합니다.
    /// </summary>
    /// Caller: NikkeAttackState.Execute()
    public override void ProcessCombat(CombatNikke owner, Vector3 targetWorldPos, bool isTargetValid)
    {
        bool isAuto = CombatMode.Value == NikkeClone.Utils.eNikkeCombatMode.Auto;
        if (!isAuto || isTargetValid)
        {
            Update(owner, targetWorldPos);
        }
    }

    /// <summary>
    /// 실제 격발 수행 (하위 클래스에서 디테일 구현)
    /// </summary>
    protected abstract void TryFire(CombatNikke owner, Vector3 targetWorldPos);
}
