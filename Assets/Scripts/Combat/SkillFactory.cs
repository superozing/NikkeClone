using System;

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
        // Phase 10 테스트: 현재는 모든 스킬 ID에 대해 KillCount 패시브를 생성합니다.
        // Phase 14: 실제 DB의 스킬 ID와 클래스를 매핑하는 테이블/딕셔너리 구조가 들어올 예정입니다.

        switch (skillID)
        {
            // Phase 10 테스트: 현재는 모든 스킬 ID에 대해 KillCount 패시브를 생성합니다.
            // 아래 케이스들은 나중에 개별 클래스가 생성되면 그때 교체합니다.
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
            case 6:
            case 7:
            case 8:
            case 9:
            case 10:
            case 11:
            case 12:
            case 13:
            case 14:
            case 15:
            case 16:
            case 17:
            case 18:
            case 19:
            case 20:
            case 21:
            case 22:
            case 23:
            case 24:
            case 25:
            case 26:
            case 27:
            case 28:
            case 29:
            case 30:
            case 31:
            case 32:
            case 33:
            case 34:
            case 35:
            case 36:
            case 37:
            case 38:
            case 39:
            case 40:
            case 41:
            case 42:
            case 43:
            case 44:
            case 45:
                return new Passive_Test();
            default:
                UnityEngine.Debug.LogWarning($"[SkillFactory] Unknown Skill ID: {skillID}");
                return null;
        }
    }
}
