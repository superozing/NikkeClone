using System.Collections.Generic;

/// <summary>
/// 전투 중 발생하는 각종 통계(데미지 등)를 기록하는 시스템 클래스입니다.
/// 배열 내 NikkeCombatRecord 객체를 미리 생성하여 매 전투마다 재사용(Pooling)합니다.
/// </summary>
public class CombatStatRecordSystem
{
    // 각 니케마다 사용할 기록자 객체
    private readonly NikkeCombatRecord[] _records = new NikkeCombatRecord[5]
    {
            new NikkeCombatRecord(),
            new NikkeCombatRecord(),
            new NikkeCombatRecord(),
            new NikkeCombatRecord(),
            new NikkeCombatRecord()
    };

    private CombatTriggerSystem _triggerSystem;

    /// <summary>
    /// 시스템 초기화: 트리거 시스템을 인자로 받아 이벤트를 구독하고 레코드를 초기화합니다.
    /// </summary>
    public void Initialize(CombatTriggerSystem triggerSystem, CombatNikke[] nikkes)
    {
        _triggerSystem = triggerSystem;

        foreach (var nikke in nikkes)
        {
            if (nikke == null) continue;

            int idx = nikke.SlotIndex;
            if (idx >= 0 && idx < _records.Length)
                _records[idx].Clear(idx, nikke.NikkeName);
        }

        // 이벤트 구독
        if (_triggerSystem != null)
        {
            _triggerSystem.OnEnemyDamagedByAlly += (idx, val) => UpdateRecord(idx, r => r.TotalDamageDealt += val);
            _triggerSystem.OnAllyDamaged += (idx, val) => UpdateRecord(idx, r => r.TotalDamageTaken += val);
            _triggerSystem.OnAllyHealed += (idx, val) => UpdateRecord(idx, r => r.TotalHealReceived += val);
        }
    }

    /// <summary>
    /// 인덱스 기반으로 레코드를 찾아 업데이트하는 헬퍼 메서드입니다.
    /// </summary>
    private void UpdateRecord(int slotIndex, System.Action<NikkeCombatRecord> updateAction)
    {
        if (slotIndex >= 0 && slotIndex < _records.Length)
            updateAction?.Invoke(_records[slotIndex]);
    }

    /// <summary>
    /// 특정 슬롯의 통계 데이터를 반환합니다.
    /// </summary>
    public NikkeCombatRecord GetRecord(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _records.Length)
            return _records[slotIndex];
        return null;
    }

    /// <summary>
    /// 시스템 종료 시 참조를 정리합니다.
    /// </summary>
    public void Cleanup()
    {
        _triggerSystem = null;
    }
}
