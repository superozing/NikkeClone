/// <summary>
/// 적(랩쳐)이 사망했을 때 발동하는 트리거 핸들러입니다.
/// </summary>
public interface ITriggerOnEnemyDied : ICombatTriggerHandler
{
    /// <summary>
    /// 적이 사망했을 때 호출됩니다.
    /// </summary>
    /// <param name="deadRapture">사망한 랩쳐 인스턴스</param>
    void OnEnemyDied(CombatRapture deadRapture);
}
