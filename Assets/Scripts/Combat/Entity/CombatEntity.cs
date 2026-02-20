using UnityEngine;

/// <summary>
/// 전투에 참여하는 모든 엔터티(니케, 랩쳐)의 기반 클래스입니다.
/// 공통적인 스탯(HP, Status)을 관리합니다.
/// </summary>
public abstract class CombatEntity : MonoBehaviour
{
    // ==================== Data ====================

    /// <summary>기본 스탯 (레벨/버프 적용 전/후 관리용)</summary>
    protected StatusData _baseStatus;

    public StatusData Status => _baseStatus;

    /// <summary>현재 HP</summary>
    protected long _currentHp;

    // ==================== Properties ====================

    /// <summary>최대 HP (BaseStatus 기준)</summary>
    public long MaxHp => _baseStatus.hp;

    /// <summary>현재 HP (읽기 전용)</summary>
    public long CurrentHp => _currentHp;

    /// <summary>사망 여부</summary>
    public bool IsDead => _currentHp <= 0;

    // ==================== Events ====================

    /// <summary>
    /// HP 변경 시 발생 (CurrentHp, MaxHp)
    /// </summary>
    public event System.Action<long, long> OnHpChanged;

    // ==================== Public Methods ====================

    // Phase 3: 데미지 처리 및 사망 로직 구현
    /// <summary>
    /// 데미지를 입습니다.
    /// Caller: CombatNikke.Fire()
    /// </summary>
    /// <param name="damage">입을 데미지 양</param>
    /// <returns>실제 적용된 데미지</returns>
    public virtual long TakeDamage(long damage)
    {
        if (IsDead) return 0;

        long actualDamage = damage;// Mathf.Max(1, damage);
        _currentHp -= actualDamage;

        OnHpChanged?.Invoke(_currentHp, MaxHp);

        Debug.Log($"[{GetType().Name}] Took {actualDamage} damage. HP: {_currentHp}/{MaxHp}");

        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Die();
        }

        return actualDamage;
    }

    /// <summary>
    /// 사망 처리입니다.
    /// Caller: TakeDamage()
    /// </summary>
    public virtual void Die()
    {
        Debug.Log($"[{GetType().Name}] Died");
        // 하위 클래스에서 override하여 상태 전환 등을 처리합니다.
    }
}
