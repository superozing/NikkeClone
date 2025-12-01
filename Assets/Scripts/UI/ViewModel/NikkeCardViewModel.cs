using System;
using UnityEngine;

public class NikkeCardViewModel : ViewModelBase
{
    // 클릭 이벤트. NikkeCardViewModel을 생성하는 객체가 세팅해주어야 해요.
    public event Action<int> OnClick;

    private readonly UserNikkeData _userData;
    private readonly NikkeGameData _gameData;

    // --- View 바인딩용 ReactiveProperty ---
    public ReactiveProperty<string> Name { get; private set; } = new();
    public ReactiveProperty<int> Level { get; private set; } = new();
    public ReactiveProperty<string> CombatPowerText { get; private set; } = new();

    public ReactiveProperty<Sprite> FaceImage { get; private set; } = new();
    public ReactiveProperty<Sprite> ClassIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> CodeIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> WeaponIcon { get; private set; } = new();
    public ReactiveProperty<Sprite> BurstIcon { get; private set; } = new();

    // ------------------------------------

    public int NikkeId => _gameData.id;
    public int BurstLevel => _gameData.burstLevel; // 필터링 용도

    public NikkeCardViewModel(UserNikkeData userData, NikkeGameData gameData)
    {
        _userData = userData;
        _gameData = gameData;

        if (_userData == null || _gameData == null)
        {
            Debug.LogError("[NikkeCardViewModel] 데이터가 유효하지 않습니다.");
            return;
        }

        // 1. 기본 텍스트 정보 설정
        Name.Value = _gameData.name;

        // 2. 유저 데이터 변경 감지 (레벨 등)
        _userData.level.OnValueChanged += OnLevelChanged;
        OnLevelChanged(_userData.level.Value); // 초기값 반영

        // 3. 리소스 비동기 로드 시작
        LoadAllResources();
    }

    private void OnLevelChanged(int level)
    {
        Level.Value = level;

        // 전투력 계산 (임시 공식: 레벨 * 100 + 공격력)
        // 실제 기획 데이터가 있다면 그에 맞춰 수정 필요
        long cp = (long)level * 100 + _gameData.attack;
        CombatPowerText.Value = Utils.FormatNumber((int)cp);
    }

    /// <summary>
    /// 모든 아이콘 리소스를 비동기로 로드합니다.
    /// </summary>
    private async void LoadAllResources()
    {
        // 리소스 경로 규칙 (Naming Convention) 정의
        // 실제 에셋 번들/Addressable 경로와 일치해야 합니다.
        string facePath     = $"Assets/Textures/Nikke/{_gameData.name}_Crop";
        string classPath    = $"Assets/Textures/Icon/Class/{_gameData.nikkeClass}";
        string codePath     = $"Assets/Textures/Icon/Code/{_gameData.element}";
        string weaponPath   = $"Assets/Textures/Icon/Weapon/{_gameData.weapon?.weaponClass}";
        string burstPath    = $"Assets/Textures/Icon/Burst/burst_{_gameData.burstLevel}";

        // 병렬 로드 (모두 동시에 요청)
        var faceTask = Managers.Resource.LoadAsync<Sprite>(facePath);
        var classTask = Managers.Resource.LoadAsync<Sprite>(classPath);
        var codeTask = Managers.Resource.LoadAsync<Sprite>(codePath);
        var weaponTask = Managers.Resource.LoadAsync<Sprite>(weaponPath);
        var burstTask = Managers.Resource.LoadAsync<Sprite>(burstPath);

        // 로드 완료되는 대로 프로퍼티 갱신
        FaceImage.Value = await faceTask;
        ClassIcon.Value = await classTask;
        CodeIcon.Value = await codeTask;
        WeaponIcon.Value = await weaponTask;
        BurstIcon.Value = await burstTask;
    }

    public void OnCardClicked()
    {
        OnClick?.Invoke(_gameData.id);
    }

    protected override void OnDispose()
    {
        if (_userData != null)
        {
            _userData.level.OnValueChanged -= OnLevelChanged;
        }
        OnClick = null;
    }
}