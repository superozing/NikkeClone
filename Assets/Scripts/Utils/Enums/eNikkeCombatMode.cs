namespace NikkeClone.Utils
{
    /// <summary>
    /// 니케의 상위 전투 모드를 정의합니다.
    /// </summary>
    public enum eNikkeCombatMode
    {
        None = 0,
        Manual, // 수동 조작 상태 (공격/엄폐/재장전 하위 상태 가짐)
        Auto,   // 자동 전투 상태 (공격/엄폐/재장전 하위 상태 가짐)
        Stun,   // 기절 상태 (행동 불가)
        Dead,    // 사망 상태
        Reload  // 재장전
    }
}
