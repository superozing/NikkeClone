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
            // UpdateMode(_nikke.CurrentMode); // Removed: CurrentMode property deleted. Wait for OnModeChanged or assume default.
            // 초기 상태는 Auto라고 가정하거나, Init 시점에 이벤트가 발생하길 기대.
            // CombatNikke.InitializeAsync에서 _hfsm.ChangeMode(Auto) 호출 시 OnModeChanged 발생함.
            // ViewModel 생성 시점이 Init 전이라면 이벤트 수신 가능. 후라면?
            // -> ViewModel은 CombatSystem.InitializeHUDAsync에서 생성됨. 이는 Nikke Init 이후임.
            // 따라서 이미 Auto 상태일 것임. 수동으로 초기값 설정 필요.
            UpdateMode(eNikkeCombatMode.Auto);
            IsDead.Value = _nikke.IsDead;

            // Subscribe to events
            _nikke.OnHpChanged += UpdateHp;
            _nikke.OnModeChanged += UpdateMode;
            _nikke.OnDeath += OnNikkeDeath;
        }
    }

    private void UpdateHp(long current, long max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        HpRatio.Value = ratio;
    }

    private void UpdateMode(eNikkeCombatMode mode)
    {
        IsSelected.Value = (mode == eNikkeCombatMode.Manual);
    }

    private void OnNikkeDeath(CombatNikke nikke)
    {
        IsDead.Value = true;
        UpdateMode(eNikkeCombatMode.Dead);
    }

    protected override void OnDispose()
    {
        if (_nikke != null)
        {
            _nikke.OnHpChanged -= UpdateHp;
            _nikke.OnModeChanged -= UpdateMode;
            _nikke.OnDeath -= OnNikkeDeath;
        }
        base.OnDispose();
    }
}
