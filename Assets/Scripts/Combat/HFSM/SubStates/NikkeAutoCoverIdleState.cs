using UnityEngine;

public class NikkeAutoCoverIdleState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        // Debug.Log($"[{owner.name}] Auto Cover Idle");
    }

    public void Execute(CombatNikke owner)
    {
        // 대기 (HP 회복 등?)
    }

    public void Exit(CombatNikke owner)
    {
    }
}
