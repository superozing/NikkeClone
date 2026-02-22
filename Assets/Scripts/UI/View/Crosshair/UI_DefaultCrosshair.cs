using TMPro;
using UnityEngine;

/// <summary>
/// 기본형 무기 (AR, SMG, MG, SG) 전용 조준선입니다.
/// 탄약 텍스트만 표시하며, 차지 게이지는 없습니다.
/// Implements Section 3: UI_DefaultCrosshair (Phase 7.1 Refactor v2 Design)
/// </summary>
public class UI_DefaultCrosshair : UI_CrosshairBase
{
    [Header("Default Crosshair")]
    [SerializeField] private TMP_Text _ammoText;

    /// <summary>
    /// CurrentAmmo, MaxAmmo만 구독하여 탄약 텍스트를 갱신합니다.
    /// Caller: UI_CrosshairBase.OnEnable() / SetViewModel()
    /// </summary>
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
