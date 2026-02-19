using UnityEngine;
using NikkeClone.Utils;

/// <summary>
/// 기절(CC) 상태입니다.
/// 행동 불가 상태를 처리합니다.
/// </summary>
public class NikkeStunState : IState<CombatNikke>
{
    private float _stunDuration;
    private float _elapsedTime;

    // 기절 지속시간은 Enter 전에 설정하거나, 생성자가 아니라 별도 메서드로 주입 받아야 함.
    // 하지만 IState.Enter의 파라미터는 owner뿐임.
    // -> CombatNikke에게 StunDuration 프로퍼티를 두거나,
    //    StunState 객체 자체를 매번 새로 만들어서 데이터를 넣거나,
    //    NikkeHFSM이 관리하거나.

    // 여기서는 간단히 CombatNikke가 StunInfo를 가지고 있다고 가정
    // 혹은 하드코딩 (Phase 6.1은 구조만 확인)

    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Stunned!");

        // TODO: owner.StunDuration 가져오기
        _stunDuration = 2.0f; // 임시
        _elapsedTime = 0f;

        owner.View.UpdateVisualState(eNikkeState.Stunned);
    }

    public void Execute(CombatNikke owner)
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime >= _stunDuration)
        {
            // 스턴 종료 -> 이전 상태로 복귀?
            // NikkeHFSM에게 알림.
            // 하지만 IState는 HFSM을 모름 (설계상)
            // Owner에게 "스턴 끝났다"고 알리면 Owner가 판단해서 Manual/Auto 복귀
            owner.SetCombatMode(eNikkeCombatMode.Auto);
        }
    }

    public void Exit(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Stun Recovered");
    }
}
