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

    /// <summary>현재 HP</summary>
    protected long _currentHp;

    // ==================== Properties ====================

    /// <summary>최대 HP (BaseStatus 기준)</summary>
    public long MaxHp => _baseStatus.hp;

    /// <summary>현재 HP (읽기 전용)</summary>
    public long CurrentHp => _currentHp;

    /// <summary>사망 여부</summary>
    public bool IsDead => _currentHp <= 0;

    // ==================== Public Methods ====================

    // Phase 2: 현재는 공통 메서드가 없습니다.
    // 추후 데미지 처리(TakeDamage) 및 사망 처리(Die) 로직이 추가될 예정입니다.

    // Phase 3: 데미지 처리 시 구현
    // public virtual long TakeDamage(long damage)
    // Caller: CombatRapture의 Attack 로직

    // Phase 3: 사망 처리 시 구현
    // public virtual void Die()
    // Caller: TakeDamage에서 HP <= 0 시 호출
}
