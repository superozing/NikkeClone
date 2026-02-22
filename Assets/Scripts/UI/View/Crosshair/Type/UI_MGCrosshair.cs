using TMPro;
using UnityEngine;

/// <summary>
/// MG 무기 전용 조준선입니다.
/// 사격 시 십자 외곽선이 벌어지는 느낌의 연출을 제공합니다.
/// Implements Section 4.2: UI_MGCrosshair (Phase 7.1 Refactor Design)
/// </summary>
public class UI_MGCrosshair : UI_CrosshairBase
{
    [Header("MG Crosshair")]
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private RectTransform _crosshairPartsRoot;

    private int _prevAmmo = -1;

    protected override void BindWeaponProperties()
    {
        Bind(_viewModel.CurrentAmmo, OnAmmoChanged);
        Bind(_viewModel.MaxAmmo, max => UpdateAmmoText(_viewModel.CurrentAmmo.Value, max));
    }

    private void UpdateAmmoText(int current, int max)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"{current} / {max}";
        }
    }

    private void OnAmmoChanged(int currentAmmo)
    {
        if (_prevAmmo != -1 && currentAmmo < _prevAmmo)
        {
            OnFire();
        }
        _prevAmmo = currentAmmo;
        UpdateAmmoText(currentAmmo, _viewModel.MaxAmmo.Value);
    }

    protected override void OnFire()
    {
        base.OnFire();
        if (_crosshairPartsRoot != null)
        {
            _crosshairPartsRoot.localScale = Vector3.one * 1.25f; // MG는 AR수준 혹은 지속 시 고정 크기 유지 등의 연출 가능 
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (_crosshairPartsRoot != null && _crosshairPartsRoot.localScale.x > 1f)
        {
            float newScale = Mathf.Lerp(_crosshairPartsRoot.localScale.x, 1f, Time.deltaTime * 10f); // MG 반동 복구 속도 조정
            if (newScale < 1.01f) newScale = 1f;
            _crosshairPartsRoot.localScale = Vector3.one * newScale;
        }
    }
}
