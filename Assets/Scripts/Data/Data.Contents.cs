using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 GameDB 데이터가 고유 ID를 갖도록 강제하는 인터페이스입니다.
/// </summary>
public interface IDataId
{
    int ID { get; }
}

// ======================= Game Data (읽기 전용) =======================

#region NikkeGameData
/// <summary>
/// 캐릭터의 모든 고정된 기본 정보를 정의합니다.
/// </summary>
[Serializable]
public class NikkeGameData : IDataId
{
    public int id;
    public string name;
    public string nikkeClass;
    public int burstLevel;
    public string element;
    public string rarity;

    public string manufacturer;
    public string squad;

    public int hp;
    public int attack;
    public int defense;

    public Color color;

    /// <summary>
    /// 무기 정보를 담는 중첩 클래스입니다.
    /// </summary>
    public WeaponData weapon;

    /// <summary>
    /// 스킬 정보 리스트를 담는 중첩 클래스입니다.
    /// </summary>
    public List<SkillData> skills;

    public int ID => id;

    // --- Converted Enum Properties ---

    public eNikkeClass ClassType => ParseClass(nikkeClass);
    public eNikkeCode CodeType => ParseCode(element);
    public eNikkeManufacturer ManufacturerType => ParseManufacturer(manufacturer);
    public eNikkeWeapon WeaponType => ParseWeapon(weapon?.weaponClass);
    public eNikkeBurst BurstType => (eNikkeBurst)Mathf.Clamp(burstLevel, 0, 3);

    // --- Helper Methods for Conversion ---

    private eNikkeClass ParseClass(string value) => value switch
    {
        "화력형" => eNikkeClass.Attacker,
        "방어형" => eNikkeClass.Defender,
        "지원형" => eNikkeClass.Supporter,
        _ => eNikkeClass.None
    };

    private eNikkeCode ParseCode(string value) => value switch
    {
        "작열" => eNikkeCode.Fire,
        "수냉" => eNikkeCode.Water,
        "풍압" => eNikkeCode.Wind,
        "전격" => eNikkeCode.Electric,
        "철갑" => eNikkeCode.Iron,
        _ => eNikkeCode.None
    };

    private eNikkeManufacturer ParseManufacturer(string value) => value switch
    {
        "엘리시온" => eNikkeManufacturer.Elysion,
        "미실리스" => eNikkeManufacturer.Missilis,
        "테트라" => eNikkeManufacturer.Tetra,
        "필그림" => eNikkeManufacturer.Pilgrim,
        "어브노멀" => eNikkeManufacturer.Abnormal,
        _ => eNikkeManufacturer.None
    };

    private eNikkeWeapon ParseWeapon(string value)
    {
        if (string.IsNullOrEmpty(value)) return eNikkeWeapon.None;
        if (Enum.TryParse(value, true, out eNikkeWeapon result))
            return result;
        return eNikkeWeapon.None;
    }
}

/// <summary>
/// 무기의 고정된 정보를 정의합니다.
/// </summary>
[Serializable]
public class WeaponData
{
    public string weaponName;
    public string weaponClass;
    public int maxAmmo;
    public float reloadTime;
    public string controlType;
    public string description;
    public float damagePercent;
}


[Serializable]
public class SkillData
{
    public int skillID;

    public int burstStage;
    public string name;
    public string description;
    public string skillTypeName; // 스킬 타입 이름 (예: "Passive", "Active")
    public float cooldown; // 초 단위
    public string skillIconPath;

    public List<SkillValueData> values;
}

[Serializable]
public class SkillValueData
{
    // 스킬 레벨을 통일했기에 min, max가 필요없어짐.
    public string value;
}
#endregion

#region ItemGameData
[Serializable]
public class ItemGameData : IDataId
{
    public int id;
    public string name;
    public string desc;
    public string iconPath;

    public int ID => id;
}
#endregion

#region MissionGameData
[Serializable]
public class MissionGameData : IDataId
{
    public int id;
    public string title;
    public string description;
    public eMissionType missionType;
    public int targetCount;

    public int rewardItemID;
    public int rewardItemCount;
    public int ID => id;
}
#endregion

// ======================= User Data (읽기/쓰기) =======================

[Serializable]
public class UserDataModel
{
    public Dictionary<int, UserNikkeData> Nikkes { get; set; } = new Dictionary<int, UserNikkeData>();
    public Dictionary<int, UserItemData> Items { get; set; } = new Dictionary<int, UserItemData>();
    public Dictionary<int, UserSquadData> Squads { get; set; } = new Dictionary<int, UserSquadData>();
    public Dictionary<int, UserMissionData> Missions { get; set; } = new Dictionary<int, UserMissionData>();
}

[Serializable]
public class UserNikkeData
{
    public int id; // 캐릭터 고유 번호
    public ReactiveProperty<int> level;
    public ReactiveProperty<int> combatPower; // 중복 계산 방지를 위한 전투력 저장

    public UserNikkeData() { }
    public UserNikkeData(int id, int level = 1)
    {
        this.id = id;
        this.level = new ReactiveProperty<int>(level);
        this.combatPower = new ReactiveProperty<int>(0);
    }
}

[Serializable]
public class UserItemData
{
    public int id; // 아이템 고유 번호
    public ReactiveProperty<int> count;

    public UserItemData() { }
    public UserItemData(int id, int count = 0)
    {
        this.id = id;
        this.count = new ReactiveProperty<int>(count);
    }
}

[Serializable]
public class UserSquadData
{
    public int id; // 스쿼드 고유 번호
    public List<int> slot; // 5개 슬롯에 배치된 캐릭터 ID 목록

    public UserSquadData() { }
    public UserSquadData(int id)
    {
        this.id = id;
        // 5개의 빈 슬롯으로 초기화
        this.slot = new List<int>(5) { -1, -1, -1, -1, -1 }; // -1을 빈 슬롯으로 가정
    }

    // 편집 취소용 깊복
    public UserSquadData Clone()
    {
        var clone = new UserSquadData(this.id);
        // 리스트 내부 값 복사
        clone.slot = new List<int>(this.slot);
        return clone;
    }

    /// <summary>
    /// 슬롯 데이터가 변경되었을 때 발생하는 이벤트입니다.
    /// </summary>
    [field: NonSerialized]
    public event Action OnSlotChanged;

    /// <summary>
    /// 슬롯 변경을 알립니다. slot 리스트 수정 후 반드시 호출해야 합니다.
    /// </summary>
    public void NotifySlotChanged()
    {
        OnSlotChanged?.Invoke();
    }
}

[Serializable]
public class UserMissionData
{
    public int id;
    public ReactiveProperty<eMissionState> state; // 미션 진행도
    public ReactiveProperty<int> currentCount; // 현재 개수

    public UserMissionData() { }
    public UserMissionData(int id)
    {
        this.id = id;
        this.state = new ReactiveProperty<eMissionState>(eMissionState.InProgress);
        this.currentCount = new ReactiveProperty<int>(0);
    }
}

// ======================= Campaign Game Data =======================

#region Shared Data Structures

/// <summary>
/// Nikke와 Rapture 공용 스테이터스 구조체입니다.
/// </summary>
[Serializable]
public class StatusData
{
    public long hp;
    public long attack;
    public long defense;
}

#endregion

#region Stage Game Data

[Serializable]
public class StageGameData : IDataId
{
    public int id;
    public string name;
    public eStageType stageType;
    public int weaknessCode;            // 약점코드 (eNikkeCode 대응)

    public RangeRatioData adequateRange; // 적정 사거리 비율
    public int referenceCombatPower;     // 기준 전투력

    public List<RewardData> rewards;     // 보상 정보
    public int stageBattleDataId;        // StageBattleGameData 참조 ID

    public int ID => id;
}

[Serializable]
public class RangeRatioData
{
    public int near;  // 0~100
    public int mid;   // 0~100
    public int far;   // 0~100
    // 합계 = 100
}

[Serializable]
public class RewardData
{
    public int itemId;
    public int count;
}

#endregion

#region Stage Battle Game Data

[Serializable]
public class StageBattleGameData : IDataId
{
    public int id;
    public int[] phaseIds;                  // 페이즈 ID 배열 (순서대로)
    public float[] phaseProgressWeights;    // 각 페이즈 완료 시 진행률 가중치 (합계 1.0)
    public int[] appearingRaptureIds;       // 등장하는 모든 랩쳐 ID (캐싱용, 후순위)

    public int ID => id;

    /// <summary>
    /// 페이즈 수를 반환합니다.
    /// </summary>
    public int PhaseCount => phaseIds?.Length ?? 0;
}

#endregion

#region Phase Game Data

[Serializable]
public class PhaseGameData : IDataId
{
    public int id;
    public bool isTargetPhase;              // 타겟이 등장하는 페이즈 여부
    public int[] appearingRaptureIds;       // 이 페이즈에 등장하는 랩쳐 ID 목록 (캐싱용)
    public List<PhaseSpawnEntry> spawns;    // 스폰 엔트리 목록

    public int ID => id;

    /// <summary>
    /// 등장 랩쳐 수를 반환합니다.
    /// </summary>
    public int SpawnCount => spawns?.Count ?? 0;
}

[Serializable]
public class PhaseSpawnEntry
{
    public int raptureId;               // 등장할 랩쳐 ID
    public float spawnDelaySec;         // 페이즈 시작 N초 후 스폰
    public string spawnerId;            // "{Distance}_{Air/Ground}_{1~3}" 형식
    public eAppearPosition appearPosition; // 등장 방향
}

#endregion

#region Rapture Game Data

[Serializable]
public class RaptureGameData : IDataId
{
    public int id;
    public string name;
    public int grade;                       // 랩쳐 등급
    public int elementCode;                 // 속성코드 (eNikkeCode 대응)

    public List<RaptureSkillData> skills;   // 스킬 목록
    public StatusData status;               // 기본 스테이터스

    public int ID => id;
}

[Serializable]
public class RaptureSkillData
{
    public string name;
    public string iconPath;     // Addressable Key 또는 Resources 경로
    public string description;
}

#endregion
