using System;
using UnityEngine;

public class NikkeCardViewModel : ViewModelBase
{
    // 클릭 이벤트는 이 ViewModel을 소유한 부모(ScrollViewModel)가 구독하여 처리합니다.
    public event Action<int> OnClick;

    private readonly UserNikkeData _userData;
    private readonly NikkeGameData _gameData;

    public int NikkeId => _gameData.id;
    public int BurstLevel => _gameData.burstLevel;

    // --- View Binding Properties ---
    public ReactiveProperty<string> Name { get; private set; } = new();
    public ReactiveProperty<int> Level { get; private set; } = new();
    public ReactiveProperty<string> CombatPowerText { get; private set; } = new();

    public ReactiveProperty<Sprite> FaceImage { get; private set; } = new();
    public ReactiveProperty<Sprite> ClassIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> CodeIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> WeaponIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> BurstIcon { get; private set; } = new();
    // -------------------------------

    // 정렬을 위한 Getter
    public int CurrentLevel => _userData.level.Value;
    public long CombatPower => CalculateCP(_userData.level.Value, _gameData.attack);
    public string NikkeName => _gameData.name;

    public NikkeCardViewModel(UserNikkeData userData, NikkeGameData gameData)
    {
        _userData = userData;
        _gameData = gameData;

        Name.Value = _gameData.name;

        // 레벨 변경 구독
        _userData.level.OnValueChanged += OnLevelChanged;
        OnLevelChanged(_userData.level.Value);

        LoadAllResources();
    }

    private void OnLevelChanged(int level)
    {
        Level.Value = level;
        CombatPowerText.Value = Utils.FormatNumber((int)CalculateCP(level, _gameData.attack));
    }

    private long CalculateCP(int level, int attack)
    {
        // 임시 전투력 공식: 레벨 * 100 + 공격력
        return (long)level * 100 + attack;
    }

    /// <summary>
    /// 모든 아이콘 리소스를 비동기로 로드합니다.
    /// </summary>
    private async void LoadAllResources()
    {
        string facePath = $"Assets/Textures/Nikke/{_gameData.name}_Crop"; // 상반신 Crop 이미지 가정
        string classPath = $"Assets/Textures/Icon/Class/{_gameData.nikkeClass}";
        string codePath = $"Assets/Textures/Icon/Code/{_gameData.element}";
        string weaponPath = $"Assets/Textures/Icon/Weapon/{_gameData.weapon?.weaponClass}";
        string burstPath = $"Assets/Textures/Icon/Burst/burst_{_gameData.burstLevel}";

        FaceImage.Value = await Managers.Resource.LoadAsync<Sprite>(facePath);
        ClassIcon.Value = await Managers.Resource.LoadAsync<Sprite>(classPath);
        CodeIcon.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);
        WeaponIcon.Value = await Managers.Resource.LoadAsync<Sprite>(weaponPath);
        BurstIcon.Value = await Managers.Resource.LoadAsync<Sprite>(burstPath);
    }

    public void OnCardClicked()
    {
        OnClick?.Invoke(_gameData.id);
    }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.level.OnValueChanged -= OnLevelChanged;

        OnClick = null;
    }
}