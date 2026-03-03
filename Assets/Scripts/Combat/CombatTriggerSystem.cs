using System;
using NikkeClone.Utils;

/// <summary>
/// 전투 내 발생하는 이벤트를 중계하고 시스템 간의 결합도를 낮추는 트리거 시스템입니다.
/// 외부 시스템(WaveSystem, Weapons)을 관찰(Observe)하여 명시적인 이벤트를 재방출합니다.
/// </summary>
public class CombatTriggerSystem
{
    // ==========================================
    // 1. 명시적 이벤트 (Explicit Events)
    // ==========================================

    /// <summary>아군(니케)이 적을 적중시켰을 때 발생. 파라미터: 공격자 슬롯 인덱스(0~4)</summary>
    public event Action<int> OnAllyHitEnemy;

    /// <summary>랩쳐(적)가 사망했을 때 발생. 파라미터: 사망한 랩쳐 객체</summary>
    public event Action<CombatRapture> OnEnemyDied;

    /// <summary>버스트 스킬이 사용되었을 때 발생. 파라미터: 시전자 인덱스, 버스트 단계</summary>
    public event Action<int, eBurstStage> OnBurstSkillUsed;

    /// <summary>아군(니케)이 피격되었을 때 발생. 파라미터: 피격자 슬롯 인덱스, 데미지량</summary>
    public event Action<int, long> OnAllyDamaged;

    /// <summary>아군(니케)이 회복되었을 때 발생. 파라미터: 회복 대상 슬롯 인덱스, 회복량</summary>
    public event Action<int, long> OnAllyHealed;

    /// <summary>아군(니케)이 적에게 데미지를 입혔을 때 발생. 파라미터: 공격자 슬롯 인덱스, 데미지량</summary>
    public event Action<int, long> OnEnemyDamagedByAlly;


    // ==========================================
    // 2. 초기화 및 외부 컴포넌트 관찰 설정
    // ==========================================

    /// <summary>
    /// 외부 핵심 시스템들을 주입받아 이벤트를 관찰 대상으로 연결합니다.
    /// </summary>
    public void Initialize(CombatWaveSystem waveSystem, IWeapon[] nikkeWeapons, CombatBurstSystem burstSystem)
    {
        // 1. 랩쳐 처치 이벤트 바인딩
        if (waveSystem != null)
        {
            waveSystem.OnRaptureDied += HandleRaptureDied;
        }

        // 2. 아군 무기 타격 이벤트 바인팅
        if (nikkeWeapons != null)
        {
            for (int i = 0; i < nikkeWeapons.Length; i++)
            {
                if (nikkeWeapons[i] is WeaponBase weapon)
                {
                    int slotIdx = i; // Closure capture 방지
                    weapon.OnHit += (_) => HandleAllyHit(slotIdx);
                }
            }
        }

        // 3. 버스트 시스템 이벤트 바인딩
        if (burstSystem != null)
        {
            burstSystem.OnBurstTriggered += (idx, stage) => OnBurstSkillUsed?.Invoke(idx, stage);
        }
    }

    // ==========================================
    // 3. 내부 핸들러 (이벤트 재방출)
    // ==========================================

    private void HandleRaptureDied(CombatRapture rapture)
    {
        OnEnemyDied?.Invoke(rapture);
    }

    private void HandleAllyHit(int attackerIdx)
    {
        OnAllyHitEnemy?.Invoke(attackerIdx);
    }

    /// <summary>
    /// 시스템 종료 시 모든 관찰 연결을 정리합니다.
    /// </summary>
    public void Cleanup()
    {
        // Note: 통상적으로 전투 시스템 수명과 함께 하므로 Clear만 수행
        OnAllyHitEnemy = null;
        OnEnemyDied = null;
        OnBurstSkillUsed = null;
    }
}

