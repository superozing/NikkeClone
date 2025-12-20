using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class NikkeDetailStatusViewModel : ViewModelBase
{
    // 데이터 소스
    private readonly NikkeGameData _gameData;
    private readonly UserNikkeData _userData;

    // --- View 바인딩 프로퍼티 ---

    // 텍스트 정보
    public ReactiveProperty<string> LevelText { get; private set; } = new();
    public ReactiveProperty<string> Name { get; private set; } = new();
    public ReactiveProperty<string> CombatPower { get; private set; } = new();
    public ReactiveProperty<string> Squad { get; private set; } = new();

    // 스테이터스 (체력, 공격력, 방어력)
    public ReactiveProperty<string> HP { get; private set; } = new();
    public ReactiveProperty<string> Attack { get; private set; } = new();
    public ReactiveProperty<string> Defense { get; private set; } = new();

    // 이미지 리소스 (스프라이트)
    public ReactiveProperty<Sprite> RarityIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> BurstIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> CodeIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> ClassIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> WeaponIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> ManufacturerIcon { get; private set; } = new();

    public NikkeDetailStatusViewModel(int nikkeId)
        : this(Managers.Data.Get<NikkeGameData>(nikkeId),
               Managers.Data.UserData.Nikkes.ContainsKey(nikkeId) ? Managers.Data.UserData.Nikkes[nikkeId] : null)
    {
    }

    public NikkeDetailStatusViewModel(NikkeGameData gameData, UserNikkeData userData)
    {
        _gameData = gameData;
        _userData = userData;

        if (_gameData == null || _userData == null)
        {
            Debug.LogError($"[NikkeDetailStatusViewModel] 필수 데이터가 누락되었습니다. GameData: {_gameData != null}, UserData: {_userData != null}");
            return;
        }

        // 2. 고정 데이터 설정
        Name.Value = _gameData.name;
        Squad.Value = _gameData.squad;

        // 3. 동적 데이터 구독
        _userData.level.OnValueChanged += OnLevelChanged;
        OnLevelChanged(_userData.level.Value);
        _userData.combatPower.OnValueChanged += OnCombatPowerChanged;
        OnCombatPowerChanged(_userData.combatPower.Value);

        // 4. 리소스 비동기 로드
        LoadResources();
    }

    private void OnLevelChanged(int level)
    {
        LevelText.Value = $"Lv.{level}";

        // 기본 값 * 레벨
        HP.Value = (_gameData.hp * level).ToString();
        Attack.Value = (_gameData.attack * level).ToString();
        Defense.Value = (_gameData.defense * level).ToString();
    }

    private void OnCombatPowerChanged(int cp) => CombatPower.Value = cp.ToString();

    private async void LoadResources()
    {
        // 1. 버스트 아이콘
        string burstPath = $"Assets/Textures/Icon/Burst/burst_{_gameData.burstLevel}";
        BurstIcon.Value = await Managers.Resource.LoadAsync<Sprite>(burstPath);

        // 2. 속성(코드) 아이콘
        string codePath = $"Assets/Textures/Icon/Code/{_gameData.element}";
        CodeIcon.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);

        // 3. 클래스 아이콘
        string classPath = $"Assets/Textures/Icon/Class/{_gameData.nikkeClass}";
        ClassIcon.Value = await Managers.Resource.LoadAsync<Sprite>(classPath);

        // 4. 무기 아이콘
        string weaponPath = $"Assets/Textures/Icon/Weapon/{_gameData.weapon?.weaponClass}";
        WeaponIcon.Value = await Managers.Resource.LoadAsync<Sprite>(weaponPath);

        // 5. 희귀도 아이콘
        // string rarityPath = $"Assets/Textures/Icon/Rarity/{_gameData.rarity}";
        RarityIcon.Value = await Managers.Resource.LoadAsync<Sprite>("DORO"); //rarityPath);

        // 6. 기업 아이콘
        // string manufacturerPath = $"Assets/Textures/Icon/Manufacturer/{_gameData.manufacturer}";
        ManufacturerIcon.Value = await Managers.Resource.LoadAsync<Sprite>("DORO"); //manufacturerPath);
    }

    /// <summary>
    /// 레벨업 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnClickLevelUp()
    {
        Debug.Log($"[NikkeDetailStatusViewModel] 레벨업 버튼 클릭 (현재 레벨: {_userData.level.Value})");
        // 레벨업 팝업 구현 예정
    }

    protected override void OnDispose()
    {
        if (_userData != null)
        {
            _userData.level.OnValueChanged -= OnLevelChanged;
            _userData.combatPower.OnValueChanged -= OnCombatPowerChanged;
        }
    }
}