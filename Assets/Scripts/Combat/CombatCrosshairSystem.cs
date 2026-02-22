using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 조준선 UI를 관리하는 시스템입니다. CombatSystem 하위에 배치됩니다.
/// UIManager를 통해 조준선 프리팹들을 생성하고, Dictionary로 관리하며,
/// 무기 전환 시 활성 조준선을 교체합니다.
/// 모든 조준선은 하나의 CrosshairViewModel을 공유합니다.
/// Implements Section 3: Combat Layer (Phase 7.1 Refactor v2 Design)
/// </summary>
public class CombatCrosshairSystem : MonoBehaviour
{
    private Dictionary<eNikkeWeapon, UI_CrosshairBase> _crosshairMap;
    private UI_CrosshairBase _activeCrosshair;
    private CrosshairViewModel _viewModel;

    /// <summary>
    /// 스쿼드 내 존재하는 유효한 무기 타입의 크로스헤어 프리팹만 로드하고 Dictionary에 캐싱합니다.
    /// 초기에 모든 크로스헤어는 Hide 상태를 유지합니다.
    /// Caller: CombatSystem.InitializeAsync()
    /// </summary>
    public async Task InitializeAsync(IEnumerable<eNikkeWeapon> squadWeaponTypes)
    {
        _crosshairMap = new Dictionary<eNikkeWeapon, UI_CrosshairBase>();
        _viewModel = new CrosshairViewModel();

        foreach (var weaponType in squadWeaponTypes)
        {
            if (_crosshairMap.ContainsKey(weaponType)) continue;

            UI_CrosshairBase crosshair = null;

            switch (weaponType)
            {
                case eNikkeWeapon.AR:
                    crosshair = await Managers.UI.ShowAsync<UI_ARCrosshair>(_viewModel);
                    break;
                case eNikkeWeapon.SR:
                    crosshair = await Managers.UI.ShowAsync<UI_SRCrosshair>(_viewModel);
                    break;
                case eNikkeWeapon.SMG:
                    crosshair = await Managers.UI.ShowAsync<UI_SMGCrosshair>(_viewModel);
                    break;
                case eNikkeWeapon.SG:
                    crosshair = await Managers.UI.ShowAsync<UI_SGCrosshair>(_viewModel);
                    break;
                case eNikkeWeapon.RL:
                    crosshair = await Managers.UI.ShowAsync<UI_RLCrosshair>(_viewModel);
                    break;
                case eNikkeWeapon.MG:
                    crosshair = await Managers.UI.ShowAsync<UI_MGCrosshair>(_viewModel);
                    break;
                default:
                    Debug.LogWarning($"[CombatCrosshairSystem] 지원되지 않는 무기 타입: {weaponType}");
                    continue;
            }

            if (crosshair != null)
            {
                _crosshairMap[weaponType] = crosshair;
                crosshair.Hide(); // 초기 비활성화
            }
        }

        Debug.Log($"[CombatCrosshairSystem] 초기화 완료. 등록된 타입 수: {_crosshairMap.Count}");
    }

    /// <summary>
    /// 대상 무기 타입에 맞는 크로스헤어로 UI를 전환합니다. 기존 활성화된 조준선은 숨김 처리합니다.
    /// Caller: CombatSystem.SetCrosshairWeapon()
    /// </summary>
    public void SwitchCrosshair(IWeapon weapon)
    {
        // 기존 조준선 비활성화
        if (_activeCrosshair != null)
        {
            _activeCrosshair.Hide();
            _activeCrosshair = null;
        }

        if (weapon == null) return;

        // ViewModel에 새 무기 세팅 (구독 교체)
        _viewModel.SetWeapon(weapon);

        // Dictionary에서 해당 무기 타입의 조준선 조회 및 활성화
        if (_crosshairMap.TryGetValue(weapon.WeaponType, out var newCrosshair))
        {
            _activeCrosshair = newCrosshair;
            _activeCrosshair.Show();
        }
        else
        {
            Debug.LogWarning($"[CombatCrosshairSystem] 등록되지 않은 무기 타입: {weapon.WeaponType}");
        }
    }

    /// <summary>
    /// 시스템 정리. 모든 조준선 UI를 닫고 맵을 정리합니다.
    /// Caller: CombatSystem.Cleanup()
    /// </summary>
    public void Cleanup()
    {
        _activeCrosshair = null;

        if (_crosshairMap != null)
        {
            var uniqueCrosshairs = new HashSet<UI_CrosshairBase>(_crosshairMap.Values);
            foreach (var crosshair in uniqueCrosshairs)
            {
                if (crosshair != null)
                {
                    Managers.UI.Close(crosshair);
                }
            }
            _crosshairMap.Clear();
        }

        _viewModel = null;
    }
}
