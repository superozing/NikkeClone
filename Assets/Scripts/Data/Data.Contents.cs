using System;
using System.Collections.Generic;

/// <summary>
/// 모든 GameDB 데이터가 고유 ID를 갖도록 강제하는 인터페이스입니다.
/// </summary>
public interface IDataWithId
{
    int ID { get; }
}

// ======================= Game Data (읽기 전용) =======================

[Serializable]
public class StatData : IDataWithId
{
    public int id;
    public string name;
    public float maxHp;
    public float attack;

    public int ID => id;
}

[Serializable]
public class ItemData : IDataWithId
{
    public int id;
    public string name;
    public string description;
    public string iconPath;

    public int ID => id;
}


// ======================= User Data (읽기/쓰기) =======================

[Serializable]
public class UserDataModel
{
    public ReactiveProperty<int> Gold;
    public ReactiveProperty<int> Dia;
    public Dictionary<int, int> Inventory;
    public Dictionary<int, UserCharacterData> Characters;

    public HashSet<int> AcquiredCharacters; // 획득한 캐릭터 아이디
}

[Serializable]
public class UserCharacterData
{
    public int characterId;
    public ReactiveProperty<int> level;
    public ReactiveProperty<int> skill1Level;
    public ReactiveProperty<int> skill2Level;

    public UserCharacterData() { }

    public UserCharacterData(int characterId, int level = 1)
    {
        this.characterId = characterId;
        this.level = new ReactiveProperty<int>(level);
        this.skill1Level = new ReactiveProperty<int>(1);
        this.skill2Level = new ReactiveProperty<int>(1);
    }
}