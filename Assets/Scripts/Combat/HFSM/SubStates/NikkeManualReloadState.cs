using UnityEngine;

public class NikkeManualReloadState : IState<CombatNikke>
{
    private float _reloadTimer;
    private bool _isReloaded;

    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Start Reloading");
        owner.View.UpdateVisualState(eNikkeState.Reload);
        _reloadTimer = 0f;
        _isReloaded = false;
    }

    public void Execute(CombatNikke owner)
    {
        _reloadTimer += Time.deltaTime;

        if (!_isReloaded && _reloadTimer >= owner.Weapon.ReloadTime)
        {
            // 재장전 완료
            owner.Weapon.Reload();
            _isReloaded = true;

            // 완료 후 Cover로 복귀해야 함.
            // Owner에게 알림 -> Owner가 상위 상태(Manual)에게 알림?
            // ManualState가 Update에서 체크하는 방식은 아님 (ReloadState 내부는 모름)

            // "재장전 끝나면 엄폐로 간다"는 누가 결정?
            // ManualState의 Update에서 owner.IsReloading 같은 걸 체크? 
            // 아니면 Callback?

            // 여기선 Owner의 메서드를 호출해서 상태를 바꾸도록 유도
            // owner.OnReloadComplete(); // REMOVED: Wrapper removed. Handled by owner polling or event.
            // OnReloadComplete() 내부에서 ManualState가 있다면 Cover로 바꾸게끔...
            // 하지만 ManualState private machine에 접근이 안됨.

            // *문제*: SubStack 간의 전환은 상위 State가 관장해야 깔끔함.
            // NikkeManualState.Execute()에서 owner.Weapon.CurrentAmmo == MaxAmmo 체크해서 Cover로 보낼 수도 있음.
            // -> 이게 제일 깔끔함. (Polling)
        }
    }

    public void Exit(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Finish Reloading");
    }
}
