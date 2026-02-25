using UnityEngine;
using UI;

/// <summary>
/// 조준선(Crosshair) 뷰모델입니다.
/// 활성화된 니케의 무기(IWeapon) 객체를 구독하고 상태를 View(UI_CrosshairBase 하위 클래스)에 전달합니다.
/// Implements Section 3: ViewModel Layer (Phase 7.1 Refactor Design)
/// </summary>
public class CrosshairViewModel : ViewModelBase
{
    // Refactor: WeaponType 제거 → ActiveWeapon으로 무기 객체 자체를 전달
    public ReactiveProperty<IWeapon> ActiveWeapon { get; } = new ReactiveProperty<IWeapon>(null);
    public ReactiveProperty<int> CurrentAmmo { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<int> MaxAmmo { get; } = new ReactiveProperty<int>(0);
    public ReactiveProperty<float> ChargeProgress { get; } = new ReactiveProperty<float>(0f);
    public float FullChargeMultiplier { get; private set; } = 1f;
    public ReactiveProperty<Vector2> TargetPosition { get; } = new ReactiveProperty<Vector2>(Vector2.zero);

    public ReactiveProperty<bool> IsAutoMode { get; } = new ReactiveProperty<bool>(true);

    private IWeapon _currentWeapon;

    public void SetWeapon(IWeapon weapon)
    {
        // 기존 무기 구독 해제
        if (_currentWeapon != null)
        {
            _currentWeapon.CurrentAmmo.OnValueChanged -= OnAmmoChanged;
            _currentWeapon.ChargeProgress.OnValueChanged -= OnChargeProgressChanged;
            _currentWeapon.CombatMode.OnValueChanged -= OnCombatModeChanged;
            _currentWeapon.CurrentAimScreenPosition.OnValueChanged -= OnAimPositionChanged;
        }

        _currentWeapon = weapon;

        if (_currentWeapon != null)
        {
            // Refactor: ActiveWeapon에 무기 객체 자체를 할당하여 View가 WeaponType을 직접 참조
            ActiveWeapon.Value = _currentWeapon;
            MaxAmmo.Value = _currentWeapon.MaxAmmo;
            FullChargeMultiplier = _currentWeapon.FullChargeMultiplier;

            // 새 무기 구독
            _currentWeapon.CurrentAmmo.OnValueChanged += OnAmmoChanged;
            _currentWeapon.ChargeProgress.OnValueChanged += OnChargeProgressChanged;
            _currentWeapon.CombatMode.OnValueChanged += OnCombatModeChanged;
            _currentWeapon.CurrentAimScreenPosition.OnValueChanged += OnAimPositionChanged;

            // 초기값 동기화
            OnAmmoChanged(_currentWeapon.CurrentAmmo.Value);
            OnChargeProgressChanged(_currentWeapon.ChargeProgress.Value);
            OnCombatModeChanged(_currentWeapon.CombatMode.Value);
            OnAimPositionChanged(_currentWeapon.CurrentAimScreenPosition.Value);
        }
        else
        {
            ActiveWeapon.Value = null;
        }
    }

    private void OnAmmoChanged(int ammo)
    {
        CurrentAmmo.Value = ammo;
    }

    private void OnChargeProgressChanged(float progress)
    {
        ChargeProgress.Value = progress;
    }

    private void OnCombatModeChanged(NikkeClone.Utils.eNikkeCombatMode mode)
    {
        IsAutoMode.Value = (mode == NikkeClone.Utils.eNikkeCombatMode.Auto);
    }

    private void OnAimPositionChanged(Vector2 position)
    {
        TargetPosition.Value = position;
    }

}
