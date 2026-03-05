using UI;

/// <summary>
/// 전투 엔터티의 체력 상태를 UI에 전달하는 뷰모델입니다.
/// </summary>
public class EntityHealthViewModel : ViewModelBase
{
    public ReactiveProperty<float> HpRatio { get; } = new(1f);
    public ReactiveProperty<bool> IsDead { get; } = new(false);

    private CombatEntity _entity;

    public EntityHealthViewModel(CombatEntity entity)
    {
        _entity = entity;
        if (_entity != null)
        {
            // 초기값 설정
            UpdateHp(_entity.CurrentHp, _entity.MaxHp);
            IsDead.Value = _entity.IsDead;

            // 이벤트 구독
            _entity.OnHpChanged += UpdateHp;
            _entity.OnDeath += HandleDeath;
        }
    }

    private void UpdateHp(long current, long max)
    {
        HpRatio.Value = max > 0 ? (float)current / max : 0f;
    }

    private void HandleDeath()
    {
        IsDead.Value = true;
    }

    protected override void OnDispose()
    {
        if (_entity != null)
        {
            _entity.OnHpChanged -= UpdateHp;
            _entity.OnDeath -= HandleDeath;
        }
        base.OnDispose();
    }
}
