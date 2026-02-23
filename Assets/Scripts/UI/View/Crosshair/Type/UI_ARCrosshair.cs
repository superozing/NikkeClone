using TMPro;
using UnityEngine;

/// <summary>
/// AR 무기 전용 조준선입니다.
/// 사격 시 십자 외곽선이 벌어지는 느낌의 연출을 제공합니다.
/// Implements Section 4.2: UI_ARCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_ARCrosshair : UI_CrosshairBase
{
    [Header("AR Crosshair")]
    [SerializeField] private RectTransform _crosshairPartsRoot;

    private PunchScaleUIAnimation _recoilAnim;
    private int _prevAmmo = -1;

    protected override void Awake()
    {
        base.Awake();
        if (_crosshairPartsRoot != null)
        {
            // AR 반동 애니메이션 설정 (1.3배까지 커지고 0.1초 복구)
            _recoilAnim = new PunchScaleUIAnimation(_crosshairPartsRoot, Vector3.one * 1.3f, 0.1f, 1, 0.3f);
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
