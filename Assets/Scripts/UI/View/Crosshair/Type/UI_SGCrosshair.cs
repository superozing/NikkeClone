using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// SG 무기 전용 조준선입니다.
/// 샷건의 산탄 범위를 시각적으로 표현합니다.
/// Implements Section 4.3: UI_SGCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_SGCrosshair : UI_CrosshairBase
{
    [Header("SG Crosshair")]
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private Image _spreadCircleImage;

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, ammo => UpdateAmmoText(ammo, _viewModel.MaxAmmo.Value));
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoText(_viewModel.CurrentAmmo.Value, max));
    }

    private void UpdateAmmoText(int current, int max)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{current} / {max}";
        }
    }
}
