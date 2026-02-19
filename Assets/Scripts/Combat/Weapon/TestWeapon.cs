using UnityEngine;

/// <summary>
/// 테스트용 무기 구현체입니다.
/// Phase 6.1에서는 기존 CombatScene의 Raycast 로직을 그대로 사용합니다.
/// </summary>
public class TestWeapon : IWeapon
{
    public eNikkeWeapon WeaponType => eNikkeWeapon.AR; // 임시 타입

    public bool CanFire => _currentAmmo > 0;

    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _maxAmmo;
    public float ReloadTime => _reloadTime;

    private Camera _mainCamera;
    private LayerMask _layerMask;
    private float _fireInterval = 0.1f; // 10발/초
    private float _lastFireTime;

    private int _currentAmmo;
    private int _maxAmmo;
    private float _reloadTime;

    public TestWeapon(WeaponData data)
    {
        _mainCamera = Camera.main;
        _layerMask = LayerMask.GetMask("CombatRapture");

        if (data != null)
        {
            _maxAmmo = data.maxAmmo;
            _reloadTime = data.reloadTime;
        }
        else
        {
            _maxAmmo = 60;
            _reloadTime = 1.5f;
        }
        _currentAmmo = _maxAmmo;
    }

    public void Enter(CombatNikke owner)
    {
        _lastFireTime = 0f;
    }

    public void Update(CombatNikke owner, Vector3 targetPosition)
    {
        // 클릭 해제 시 발사하므로 Update에서는 조준점 갱신 등만 처리
        // 현재는 별도 로직 없음
    }

    public void Exit(CombatNikke owner)
    {
        // 발사 (클릭 해제 시점)
        if (CanFire)
        {
            FireRaycast(owner);
        }
        else
        {
            // 탄약 부족 (사운드 재생 등?)
        }
    }

    public void Reload()
    {
        _currentAmmo = _maxAmmo;
        Debug.Log("[TestWeapon] Reloaded. Ammo: " + _currentAmmo);
    }

    public void ConsumeAmmo(int amount)
    {
        _currentAmmo = Mathf.Max(0, _currentAmmo - amount);
    }

    private void FireRaycast(CombatNikke owner)
    {
        // 탄약 소비
        ConsumeAmmo(1);

        // 마우스 위치 기반 레이캐스트 (기존 로직 유지)
        // 주의: 모바일/터치 대응 시 InputSystem 사용 필요. 여기선 Legacy Input 사용 (기존 유지)
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
        {
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && !rapture.IsDead)
            {
                // 데미지 처리
                rapture.TakeDamage(owner.Status.attack);
                // 히트 이펙트 등은 여기서 처리하거나 이벤트로?
                // TestWeapon이므로 직접 호출
                Debug.Log($"[TestWeapon] Hit Rapture: {rapture.name}. Ammo: {_currentAmmo}");
            }
        }
        else
        {
            Debug.Log($"[TestWeapon] Shot missed. Ammo: {_currentAmmo}");
        }
    }
}
