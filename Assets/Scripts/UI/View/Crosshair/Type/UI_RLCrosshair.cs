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
    [SerializeField] private Image _chargeGaugeFill;
    [SerializeField] private TMP_Text _chargeMultiplierText;
    [SerializeField] private TMP_Text _damageMultiplierText;

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, ammo => UpdateAmmoUI(ammo, _viewModel.MaxAmmo.Value));
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoUI(_viewModel.CurrentAmmo.Value, max));
        Bind(_viewModel.ChargeProgress, OnChargeRatioChanged);
    }

    private void UpdateMultiplierText(float ratio)
    {
        float multiplier = Mathf.Lerp(1.0f, _viewModel.FullChargeMultiplier, ratio);

        if (_chargeMultiplierText != null)
        {
            _chargeMultiplierText.text = $"{Mathf.RoundToInt(ratio * 100):D3}%";
        }
        if (_damageMultiplierText != null)
        {
            _damageMultiplierText.text = $"{Mathf.RoundToInt(multiplier * 100):D3}%";
        }
    }

    protected override void OnChargeRatioChanged(float ratio)
    {
        base.OnChargeRatioChanged(ratio);

        if (_chargeGaugeFill != null)
        {
            _chargeGaugeFill.fillAmount = ratio;
        }

        UpdateMultiplierText(ratio);
    }
}
