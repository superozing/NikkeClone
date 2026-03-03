using NikkeClone.Utils;

/// <summary>
/// 버스트 스킬이 사용되었을 때 발동하는 트리거 핸들러입니다.
/// </summary>
public interface ITriggerOnBurstUsed : ICombatTriggerHandler
{
    /// <summary>
    /// 버스트 스킬이 발동되었을 때 호출됩니다.
    /// </summary>
    /// <param name="casterIdx">시전자 니케의 슬롯 인덱스 (0~4)</param>
    /// <param name="stage">발동된 버스트 단계</param>
    void OnBurstUsed(int casterIdx, eBurstStage stage);
}
