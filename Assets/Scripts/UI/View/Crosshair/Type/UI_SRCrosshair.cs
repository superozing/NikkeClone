using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SR 무기 전용 조준선입니다.
/// 차지 게이지 연출 및 풀 차지 이펙트를 제공합니다.
/// Implements Section 4.1: UI_SRCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_SRCrosshair : UI_CrosshairBase
{
    [Header("SR Crosshair")]
    [SerializeField] private Image _chargeGaugeFill;
    [SerializeField] private GameObject _fullChargeEffectObj;
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

        if (_fullChargeEffectObj != null)
        {
            // ratio가 1 이상이면 풀 차지 이펙트 활성화
            _fullChargeEffectObj.SetActive(ratio >= 1f);
        }
    }
}
