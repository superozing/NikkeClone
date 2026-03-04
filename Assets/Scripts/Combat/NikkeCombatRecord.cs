/// <summary>
/// 개별 니케의 전투 통계 데이터 구조체입니다.
/// </summary>
public class NikkeCombatRecord
{
    public int SlotIndex;
    public string Name;
    public long TotalDamageDealt;
    public long TotalDamageTaken;
    public long TotalHealReceived;

    public void Clear(int slotIndex, string name)
    {
        SlotIndex = slotIndex;
        Name = name;
        TotalDamageDealt = 0;
        TotalDamageTaken = 0;
        TotalHealReceived = 0;
    }
}
