/// <summary>
/// 정렬 기준
/// </summary>
public enum eNikkeSortType
{
    CombatPower,
    Level,
}

public enum eNikkeClass
{
    None,
    Attacker,   // 화력형
    Defender,   // 방어형
    Supporter,  // 지원형
    End
}

public enum eNikkeCode
{
    None,
    Fire,       // 작열
    Water,      // 수냉
    Wind,       // 풍압
    Electric,   // 전격
    Iron,       // 철갑
    End
}

public enum eNikkeWeapon
{
    None,
    AR,
    SMG,
    SG,
    SR,
    RL,
    MG,
    End
}

public enum eNikkeManufacturer
{
    None,
    Elysion,    // 엘리시온
    Missilis,   // 미실리스
    Tetra,      // 테트라
    Pilgrim,    // 필그림
    Abnormal,   // 어브노멀
    End
}

public enum eNikkeBurst
{
    None = 0,
    Burst1 = 1,
    Burst2 = 2,
    Burst3 = 3,
    End
}