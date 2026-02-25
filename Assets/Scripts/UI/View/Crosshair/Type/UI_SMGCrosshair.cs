using TMPro;
using UnityEngine;

/// <summary>
/// SMG 무기 전용 조준선입니다.
/// 사격 시 십자 외곽선이 벌어지는 느낌의 연출을 제공합니다.
/// Implements Section 4.2: UI_SMGCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_SMGCrosshair : UI_CrosshairBase
{
    [Header("SMG Crosshair")]
    [SerializeField] private RectTransform _crosshairPartsRoot;

    private PunchScaleUIAnimation _recoilAnim;
    private int _prevAmmo = -1;

    protected override void Awake()
    {
        base.Awake();
        if (_crosshairPartsRoot != null)
        {
            // SMG 반동 애니메이션 설정 (1.4배로 크게 튀고 0.08초만에 빠르게 복구)
            _recoilAnim = new PunchScaleUIAnimation(_crosshairPartsRoot, Vector3.one * 1.4f, 0.08f, 1, 0.2f);
        }
    }

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, OnAmmoChanged);
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoUI(_viewModel.CurrentAmmo.Value, max));
    }

    private void OnAmmoChanged(int currentAmmo)
    {
        if (_prevAmmo != -1 && currentAmmo < _prevAmmo)
        {
            OnFire();
        }
        _prevAmmo = currentAmmo;
        UpdateAmmoUI(currentAmmo, _viewModel.MaxAmmo.Value);
    }

    protected override void OnFire()
    {
        base.OnFire();
        _recoilAnim?.ExecuteAsync();
    }
}
