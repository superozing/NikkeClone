using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// RL 무기 전용 조준선입니다.
/// 넓은 폭발 범위 묘사와 발사(차지) 게이지를 제공합니다.
/// Implements Section 4.1: UI_RLCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_RLCrosshair : UI_CrosshairBase
{
    [Header("RL Crosshair")]
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private Image _chargeGaugeFill;

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, ammo => UpdateAmmoText(ammo, _viewModel.MaxAmmo.Value));
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoText(_viewModel.CurrentAmmo.Value, max));
        Bind(_viewModel.ChargeProgress, OnChargeRatioChanged);
    }

    private void UpdateAmmoText(int current, int max)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{current} / {max}";
        }
    }

    protected override void OnChargeRatioChanged(float ratio)
    {
        base.OnChargeRatioChanged(ratio);

        if (_chargeGaugeFill != null)
        {
            _chargeGaugeFill.fillAmount = ratio;
        }
    }
}
