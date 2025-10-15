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
}

/// <summary>
/// 무기의 고정된 정보를 정의합니다.
/// </summary>
[Serializable]
public class WeaponData
{
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
    public string type; // 문자열 매핑용 타입
    public string minValue;
    public string maxValue;
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


// ======================= User Data (읽기/쓰기) =======================


[Serializable]
public class UserDataModel : ISerializationCallbackReceiver
{
    // Dictionary들은 NonSerialized로 설정하여 JsonUtility가 직접 처리하지 않도록 합니다.
    [NonSerialized]
    public Dictionary<int, UserNikkeData> Nikkes = new Dictionary<int, UserNikkeData>();
    [NonSerialized]
    public Dictionary<int, UserItemData> Items = new Dictionary<int, UserItemData>();
    [NonSerialized]
    public Dictionary<int, UserSquadData> Squads = new Dictionary<int, UserSquadData>();

    // JsonUtility가 직렬화할 임시 리스트들
    [SerializeField] private List<UserNikkeData> _nikkesForSave;
    [SerializeField] private List<UserItemData> _itemsForSave;
    [SerializeField] private List<UserSquadData> _squadsForSave;

    /// <summary>
    /// 데이터를 JSON으로 저장하기 직전에 Unity에 의해 호출됩니다.
    /// </summary>
    public void OnBeforeSerialize()
    {
        // 각 Dictionary의 Value들을 임시 리스트로 복사합니다.
        _nikkesForSave = new List<UserNikkeData>(Nikkes.Values);
        _itemsForSave = new List<UserItemData>(Items.Values);
        _squadsForSave = new List<UserSquadData>(Squads.Values);
    }

    /// <summary>
    /// JSON에서 데이터를 불러온 직후 Unity에 의해 호출됩니다.
    /// </summary>
    public void OnAfterDeserialize()
    {
        // 임시 리스트들의 내용으로 각 Dictionary를 다시 재구성합니다.
        Nikkes = new Dictionary<int, UserNikkeData>();
        if (_nikkesForSave != null)
        {
            foreach (var nikke in _nikkesForSave)
            {
                Nikkes.Add(nikke.id, nikke);
            }
        }

        Items = new Dictionary<int, UserItemData>();
        if (_itemsForSave != null)
        {
            foreach (var item in _itemsForSave)
            {
                Items.Add(item.id, item);
            }
        }

        Squads = new Dictionary<int, UserSquadData>();
        if (_squadsForSave != null)
        {
            foreach (var squad in _squadsForSave)
            {
                Squads.Add(squad.id, squad);
            }
        }
    }
}

[Serializable]
public class UserNikkeData
{
    public int id; // 캐릭터 고유 번호
    public ReactiveProperty<int> level;
    public ReactiveProperty<int> skill1Level;
    public ReactiveProperty<int> skill2Level;
    public ReactiveProperty<int> skill3Level;

    public UserNikkeData() { }
    public UserNikkeData(int id, int level = 1)
    {
        this.id = id;
        this.level = new ReactiveProperty<int>(level);
        this.skill1Level = new ReactiveProperty<int>(1);
        this.skill2Level = new ReactiveProperty<int>(1);
        this.skill3Level = new ReactiveProperty<int>(1);
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
}