using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// MG 무기 전용 조준선입니다.
/// 사격 시 십자 외곽선이 벌어지는 느낌의 연출을 제공합니다.
/// Implements Section 4.2: UI_MGCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_MGCrosshair : UI_CrosshairBase
{
    [Header("MG Crosshair")]
    [SerializeField] private RectTransform _crosshairPartsRoot;
    [SerializeField] private Image _gaugeImage;

    private PunchScaleUIAnimation _recoilAnim;
    private int _prevAmmo = -1;

    protected override void Awake()
    {
        base.Awake();
        if (_crosshairPartsRoot != null)
        {
            // MG 반동 애니메이션 설정 (1.2배까지 다소 작게 튀고 0.1초 복구)
            _recoilAnim = new PunchScaleUIAnimation(_crosshairPartsRoot, Vector3.one * 1.2f, 0.1f, 1, 0.1f);
        }
    }

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, OnAmmoChanged);
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoUI(_viewModel.CurrentAmmo.Value, max));
        Bind(_viewModel.ChargeProgress, OnChargeProgressChanged);
    }

    private void OnChargeProgressChanged(float progress)
    {
        if (_gaugeImage != null)
        {
            _gaugeImage.fillAmount = progress;
        }
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
