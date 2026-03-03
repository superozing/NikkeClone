using System;
using UnityEngine;

/// <summary>
/// 스킬 ID를 기반으로 실제 스킬 인스턴스를 생성하는 팩토리 클래스입니다.
/// </summary>
public static class SkillFactory
{
    /// <summary>
    /// 전달받은 skillID에 해당하는 스킬 클래스의 인스턴스를 생성하여 반환합니다.
    /// </summary>
    public static SkillBase CreateSkill(int skillID)
    {

        switch (skillID)
        {
            // TODO: 스킬 클래스 생성 시 id 값에 맞게 이 곳에 추가해야 해요.

            default:
                Debug.LogWarning($"[SkillFactory] 정의되지 않은 스킬입니다. ID: {skillID}");
                if (skillID % 3 == 0)
                    return new Burst_Test();
                else
                    return new Passive_Test();
        }
        // Phase 10 테스트: 3의 배수 ID(3, 6, 9...)를 버스트 스킬로 간주하고 Burst_Test를 반환합니다.
        // Phase 14: 실제 DB의 스킬 ID와 클래스를 매핑하는 테이블/딕셔너리 구조가 들어올 예정입니다.
    }
}
