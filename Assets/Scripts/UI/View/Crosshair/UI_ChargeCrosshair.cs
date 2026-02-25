using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 차지형 무기 (SR, RL) 전용 조준선입니다.
/// 탄약 텍스트와 차지 게이지(Fill Amount)를 표시합니다.
/// Implements Section 3: UI_ChargeCrosshair (Phase 7.1 Refactor v2 Design)
/// </summary>
public class UI_ChargeCrosshair : UI_CrosshairBase
{
    [Header("Charge Crosshair")]
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private Image _chargeGaugeFill;
    [SerializeField] private Color _defaultChargeColor = Color.white;
    [SerializeField] private Color _fullChargeColor = Color.red;

    /// <summary>
    /// CurrentAmmo, MaxAmmo, ChargeProgress를 구독합니다.
    /// Caller: UI_CrosshairBase.OnEnable() / SetViewModel()
    /// </summary>
    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, ammo => UpdateAmmoText(ammo, _viewModel.MaxAmmo.Value));
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoText(_viewModel.CurrentAmmo.Value, max));
        Bind(_viewModel.ChargeProgress, OnChargeProgressChanged);
    }

    private void UpdateAmmoText(int current, int max)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{current} / {max}";
        }
    }

    private void OnChargeProgressChanged(float progress)
    {
        if (_chargeGaugeFill == null) return;

        _chargeGaugeFill.fillAmount = progress;
        _chargeGaugeFill.color = progress >= 1f ? _fullChargeColor : _defaultChargeColor;
    }
}
