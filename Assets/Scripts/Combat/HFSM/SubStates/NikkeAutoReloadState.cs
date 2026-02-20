using UnityEngine;

public class NikkeAutoReloadState : IState<CombatNikke>
{
    private float _reloadTimer;

    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Start Auto Reload");
        owner.View.UpdateVisualState(eNikkeState.Reload);
        _reloadTimer = 0f;
    }

    public void Execute(CombatNikke owner)
    {
        _reloadTimer += Time.deltaTime;

        if (_reloadTimer >= owner.Weapon.ReloadTime)
        {
            owner.Weapon.Reload();
            // 상태 전환은 CoverState (Parent) 가 감지하여 수행
        }
    }

    public void Exit(CombatNikke owner)
    {
        // UI 갱신 등?
    }
}
