using UnityEngine;

public class NikkeManualAttackState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        if (owner.IsDead || owner.Weapon == null || !owner.Weapon.CanFire) return; // 방어 코드

        owner.View.UpdateVisualState(eNikkeState.Attack);

        // 무기 로직 시작 (발사음, 이펙트 준비 등)
        owner.Weapon?.Enter(owner);
    }

    public void Execute(CombatNikke owner)
    {
        if (owner.IsDead || owner.Weapon == null || !owner.Weapon.CanFire)
        {
            // 탄약 다 떨어지면 Reload로 전환? 
            // 이는 상위(ManualState)에서 처리하거나 여기서 요청.
            // 하지만 SubState는 상위 StateMachine을 모름.
            // Owner가 Reload를 요청하도록 해야 함.
            // owner.Reload(); // REMOVED: Managed by ManualState polling 
            // 아니, ManualState가 Update에서 탄약을 체크하고 있음.
            // 따라서 여기는 로직만 수행하면 됨.
            return;
        }

        owner.Weapon?.Update(owner);


    }

    public void Exit(CombatNikke owner)
    {
        owner.Weapon?.Exit(owner);
    }
}
