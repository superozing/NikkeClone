using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 전투에 참여하는 모든 엔터티(니케, 랩쳐)의 기반 클래스입니다.
/// 공통적인 스탯(HP, Status)을 관리합니다.
/// </summary>
public abstract class CombatEntity : MonoBehaviour
{
    // ==================== Data ====================

    /// <summary>기본 스탯 (레벨/버프 적용 전/후 관리용)</summary>
    protected StatusData _baseStatus;
    public StatusData BaseStatus => _baseStatus;

    // ==================== Controllers ====================

    /// <summary>엔터티의 통합 상태 관리자 (스탯 및 버프/디버프)</summary>
    public EntityStatusController Status { get; private set; }

    // ==================== Properties ====================

    /// <summary>현재 HP</summary>
    protected long _currentHp;

    public long MaxHp => (long)Status.Current.HP;
    public long CurrentHp => _currentHp;
    public bool IsDead => _currentHp <= 0;

    // ==================== Visuals ====================

    [SerializeField] private Transform _healthBarTrackingAnchor;
    public Transform HealthBarTrackingAnchor => _healthBarTrackingAnchor != null ? _healthBarTrackingAnchor : transform;

    // ==================== Unity Lifecycle ====================

    protected virtual void Awake()
    {
        // Note: Status는 데이터 주입 후 각 Entity 클래스에서 InitializeStatus()를 통해 초기화합니다.
    }

    protected virtual void Update()
    {
        if (!IsDead && Status != null)
        {
            Status.Tick(Time.deltaTime);
        }
    }

    /// <summary>
    /// 주입된 데이터(@_baseStatus)를 바탕으로 StatusController를 초기화합니다.
    /// </summary>
    public void InitializeStatus()
    {
        Status = new EntityStatusController(this, _baseStatus);
    }

    // ==================== Events ====================

    /// <summary>데미지를 입었을 때 발생하는 이벤트 (데미량, 현재체력, 최대체력)</summary>
    public event System.Action<long, long, long> OnDamaged;

    /// <summary>체력이 회복되었을 때 발생하는 이벤트 (회복량, 현재체력, 최대체력)</summary>
    public event System.Action<long, long, long> OnHealed;

    /// <summary>HP가 변경되었을 때 발생하는 이벤트 (현재체력, 최대체력)</summary>
    public event System.Action<long, long> OnHpChanged;

    /// <summary>사망 시 발생하는 이벤트</summary>
    public event System.Action OnDeath;

    // ==================== Public Methods ====================

    /// <summary>
    /// 데미지를 입습니다.
    /// </summary>
    /// <param name="damage">입을 데미지 양</param>
    /// <returns>실제 적용된 데미지</returns>
    public virtual long TakeDamage(long damage)
    {
        if (IsDead) return 0;

        long actualDamage = damage;
        _currentHp = System.Math.Max(0, _currentHp - actualDamage);

        if (_currentHp <= 0)
        {
            OnDamaged?.Invoke(actualDamage, _currentHp, MaxHp);
            OnHpChanged?.Invoke(_currentHp, MaxHp);
            Die();
        }
        else
        {
            OnDamaged?.Invoke(actualDamage, _currentHp, MaxHp);
            OnHpChanged?.Invoke(_currentHp, MaxHp);
        }

        return actualDamage;
    }

    /// <summary>
    /// 사망 처리입니다.
    /// </summary>
    public virtual void Die()
    {
        OnDeath?.Invoke();
        Debug.Log($"[{GetType().Name}] Died");
    }
}
