using UI;
using UnityEngine;
using NikkeClone.Utils;

public class NikkeStateViewModel : ViewModelBase
{
    // Reactive Properties for UI Binding
    public ReactiveProperty<Sprite> ProfileImage { get; } = new();
    public ReactiveProperty<float> HpRatio { get; } = new();
    public ReactiveProperty<bool> IsDead { get; } = new();
    public ReactiveProperty<bool> IsSelected { get; } = new();

    private CombatNikke _nikke;

    public NikkeStateViewModel(CombatNikke nikke)
    {
        _nikke = nikke;
        if (_nikke != null)
        {
            // Initial Data Setup
            UpdateHp(_nikke.CurrentHp, _nikke.MaxHp);

            // V2 Refactor: IsSelected를 직접 구독
            IsSelected.Value = _nikke.IsSelected.Value;
            IsDead.Value = _nikke.IsDead;

            // Subscribe to events
            _nikke.OnHpChanged += UpdateHp;
            _nikke.IsSelected.OnValueChanged += OnSelectedChanged;
            _nikke.OnDeath += OnNikkeDeath;
        }
    }

    private void UpdateHp(long current, long max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        HpRatio.Value = ratio;
    }

    private void OnSelectedChanged(bool isSelected)
    {
        IsSelected.Value = isSelected;
    }

    private void OnNikkeDeath(CombatNikke nikke)
    {
        IsDead.Value = true;
    }

    protected override void OnDispose()
    {
        if (_nikke != null)
        {
            _nikke.OnHpChanged -= UpdateHp;
            _nikke.IsSelected.OnValueChanged -= OnSelectedChanged;
            _nikke.OnDeath -= OnNikkeDeath;
        }
        base.OnDispose();
    }
}
