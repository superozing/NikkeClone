/// <summary>
/// 아군(니케)이 적을 적중시켰을 때 발동하는 트리거 핸들러입니다.
/// </summary>
public interface ITriggerOnAllyHit : ICombatTriggerHandler
{
    /// <summary>
    /// 아군이 적을 적중시켰을 때 호출됩니다.
    /// </summary>
    /// <param name="attackerIdx">공격자 니케의 슬롯 인덱스 (0~4)</param>
    void OnAllyHitEnemy(int attackerIdx);
}
