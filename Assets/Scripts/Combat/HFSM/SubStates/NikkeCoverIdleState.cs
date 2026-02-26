/// <summary>
/// 엄폐 대기 상태. 탄약이 가득 차 있을 때 대기합니다.
/// </summary>
public class NikkeCoverIdleState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        owner.UpdateState(eNikkeState.Cover);
    }
    public void Execute(CombatNikke owner) { }
    public void Exit(CombatNikke owner) { }
}
