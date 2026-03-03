using System.Collections.Generic;
using NikkeClone.Utils;

/// <summary>
/// 엔터티의 스탯 계산과 버프/디버프 생명주기를 통합 관리하는 컨트롤러입니다.
/// Dirty Flag 패턴을 사용하여 스탯 변경 시에만 최종 값을 재계산합니다.
/// </summary>
public class EntityStatusController
{
    private readonly CombatEntity _owner;

    // 원본 스탯 (데이터 테이블 기반)
    private StatGroup _baseStats = default;

    // 누적될 수정치들
    private StatGroup _addModifiers = default;
    private StatGroup _multModifiers = default;

    // 최종 계산된 스탯 (외부 노출용)
    public StatGroup Current { get; private set; } = default;

    // 적용 중인 효과 리스트
    private readonly List<ActiveEffect> _activeEffects = new List<ActiveEffect>();

    private bool _isDirty = true;

    public EntityStatusController(CombatEntity owner, StatusData baseStatus)
    {
        _owner = owner;

        // 초기 스탯 설정 (StatusData: long -> StatGroup: float)
        _baseStats.HP = baseStatus.hp;
        _baseStats.Attack = baseStatus.attack;
        _baseStats.Defense = baseStatus.defense;
        _baseStats.CriticalRate = 0; // 필요 시 전용 데이터에서 로드
        _baseStats.CriticalDamage = 1.5f; // 기본 150% 등 설정 가능

        RecalculateFinal();
    }

    /// <summary>
    /// 매 프레임 호출되어 효과의 지속시간을 갱신합니다.
    /// </summary>
    public void Tick(float deltaTime)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; --i)
        {
            var effect = _activeEffects[i];
            effect.RemainingTime -= deltaTime;

            if (effect.IsExpired)
            {
                RevertModifiers(effect);
                _activeEffects.RemoveAt(i);
                _isDirty = true;
            }
        }

        if (_isDirty)
        {
            RecalculateFinal();
            _isDirty = false;
        }
    }

    /// <summary>
    /// 새로운 효과를 추가합니다.
    /// </summary>
    public void AddEffect(EffectData data)
    {
        var newEffect = new ActiveEffect(data);
        _activeEffects.Add(newEffect);

        ApplyModifiers(newEffect);
        _isDirty = true;
    }

    /// <summary>
    /// 현재 적용 중인 모든 효과 리스트를 반환합니다.
    /// </summary>
    public IReadOnlyList<ActiveEffect> GetActiveEffects() => _activeEffects;

    /// <summary>
    /// 효과의 수정치를 누적 버퍼에 적용합니다.
    /// </summary>
    private void ApplyModifiers(ActiveEffect effect)
    {
        if (effect.Data.ModifierType == eStatModifierType.Additive)
        {
            _addModifiers += effect.Data.StatModifiers;
        }
        else
        {
            _multModifiers += effect.Data.StatModifiers;
        }
    }

    /// <summary>
    /// 효과의 수정치를 누적 버퍼에서 제거합니다.
    /// </summary>
    private void RevertModifiers(ActiveEffect effect)
    {
        if (effect.Data.ModifierType == eStatModifierType.Additive)
        {
            _addModifiers -= effect.Data.StatModifiers;
        }
        else
        {
            _multModifiers -= effect.Data.StatModifiers;
        }
    }

    /// <summary>
    /// 누적된 수정치들을 바탕으로 최종 스탯을 갱신합니다.
    /// 공식: Final = (Base + AddSum) * (1 + MultSum)
    /// </summary>
    private void RecalculateFinal()
    {
        Current = new StatGroup
        {
            HP = (_baseStats.HP + _addModifiers.HP) * (1 + _multModifiers.HP),
            Attack = (_baseStats.Attack + _addModifiers.Attack) * (1 + _multModifiers.Attack),
            Defense = (_baseStats.Defense + _addModifiers.Defense) * (1 + _multModifiers.Defense),
            CriticalRate = (_baseStats.CriticalRate + _addModifiers.CriticalRate) * (1 + _multModifiers.CriticalRate),
            CriticalDamage = (_baseStats.CriticalDamage + _addModifiers.CriticalDamage) * (1 + _multModifiers.CriticalDamage)
        };
    }
}
