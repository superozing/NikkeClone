using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class NikkeIconViewModel : ViewModelBase
{
    private NikkeGameData _gameData;
    private UserNikkeData _userData;

    public int NikkeId { get; private set; } = -1;

    /// <summary>
    /// 니케의 코드(속성) 타입입니다. Empty 상태일 경우 None을 반환합니다.
    /// </summary>
    public eNikkeCode CodeType => _gameData?.CodeType ?? eNikkeCode.None;

    /// <summary>
    /// 니케의 무기 타입입니다. Empty 상태일 경우 None을 반환합니다.
    /// </summary>
    public eNikkeWeapon WeaponType => _gameData?.WeaponType ?? eNikkeWeapon.None;

    /// <summary>
    /// 니케 슬롯이 비어있는지 여부입니다. (GameData가 없으면 비어있음)
    /// </summary>
    public bool IsSlotEmpty => _gameData == null;

    // --- View Binding Properties ---
    public ReactiveProperty<bool> IsEmpty { get; private set; } = new(true);
    public ReactiveProperty<Sprite> FaceSprite { get; private set; } = new();
    public ReactiveProperty<Sprite> BurstIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> CodeIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> WeaponIcon { get; private set; } = new();
    public ReactiveProperty<Color> RarityColor { get; private set; } = new(Color.white);
    public ReactiveProperty<string> LevelText { get; private set; } = new("");

    public NikkeIconViewModel()
    {
    }

    /// <summary>
    /// 니케 데이터를 설정하고 리소스를 로드합니다.
    /// -1이나 유효하지 않은 ID가 들어오면 빈 상태로 설정됩니다.
    /// </summary>
    public async Task SetNikke(int nikkeId)
    {
        NikkeId = nikkeId;
        if (NikkeId == -1)
        {
            ClearData();
            return;
        }

        _gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
        _userData = Managers.Data.UserData.Nikkes.ContainsKey(nikkeId) ? Managers.Data.UserData.Nikkes[nikkeId] : null;

        if (_gameData == null)
        {
            ClearData();
            return;
        }

        IsEmpty.Value = false;

        // 데이터 바인딩
        if (_userData != null)
        {
            _userData.level.OnValueChanged += OnLevelChanged;
            OnLevelChanged(_userData.level.Value);
        }

        // 희귀도 색상 설정 (임시 하드코딩, 실제론 테이블이나 유틸 필요)
        // SR: 보라, SSR: 금색
        Color frameColor = Color.white;
        if (_gameData.rarity == "SSR") frameColor = new Color(1f, 0.84f, 0f); // Gold
        else if (_gameData.rarity == "SR") frameColor = new Color(0.6f, 0.2f, 0.8f); // Purple
        RarityColor.Value = frameColor;

        await LoadResources();
    }

    private void ClearData()
    {
        if (_userData != null)
            _userData.level.OnValueChanged -= OnLevelChanged;

        _gameData = null;
        _userData = null;

        FaceSprite.Value = null;
        BurstIcon.Value = null;
        CodeIcon.Value = null;
        WeaponIcon.Value = null;
        LevelText.Value = "";

        IsEmpty.Value = true;
    }

    private void OnLevelChanged(int level)
    {
        LevelText.Value = $"Lv.{level}";
    }

    private async Task LoadResources()
    {
        if (_gameData == null) return;

        // Face Image ({Name}_Face)
        string facePath = $"Assets/Textures/Nikke/{_gameData.name}_Face";
        FaceSprite.Value = await Managers.Resource.LoadAsync<Sprite>(facePath);

        // Icons
        string burstPath = $"Assets/Textures/Icon/Burst/burst_{_gameData.burstLevel}";
        BurstIcon.Value = await Managers.Resource.LoadAsync<Sprite>(burstPath);

        string codePath = $"Assets/Textures/Icon/Code/{_gameData.element}";
        CodeIcon.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);

        string weaponPath = $"Assets/Textures/Icon/Weapon/{_gameData.weapon?.weaponClass}";
        WeaponIcon.Value = await Managers.Resource.LoadAsync<Sprite>(weaponPath);
    }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.level.OnValueChanged -= OnLevelChanged;
    }
}
