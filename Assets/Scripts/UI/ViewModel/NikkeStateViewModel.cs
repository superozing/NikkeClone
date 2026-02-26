using UI;
using UnityEngine;
using NikkeClone.Utils;
using System.Threading.Tasks;

public class NikkeStateViewModel : ViewModelBase
{
    // Reactive Properties for UI Binding
    public ReactiveProperty<Sprite> ProfileImage { get; } = new();
    public ReactiveProperty<float> HpRatio { get; } = new();
    public ReactiveProperty<eNikkeState> CurrentState { get; } = new();
    public ReactiveProperty<bool> IsSelected { get; } = new();

    // Phase 5: 신규 연동 데이터
    public ReactiveProperty<Sprite> CodeIcon { get; } = new();
    public ReactiveProperty<int> CurrentAmmo { get; } = new();
    public ReactiveProperty<int> MaxAmmo { get; } = new();
    public eNikkeCode CodeType { get; private set; }

    private CombatNikke _nikke;

    public NikkeStateViewModel(CombatNikke nikke)
    {
        _nikke = nikke;
        if (_nikke != null)
        {
            // Initial Data Setup
            UpdateHp(_nikke.CurrentHp, _nikke.MaxHp);

            // V2 Refactor: ReactiveProperty 직접 구독
            CurrentState.Value = _nikke.State.Value;
            IsSelected.Value = _nikke.IsSelected.Value;

            // Subscribe to events
            _nikke.OnHpChanged += UpdateHp;
            _nikke.State.OnValueChanged += OnStateChanged;
            _nikke.IsSelected.OnValueChanged += OnSelectedChanged;

            // Phase 5: 신규 바인딩
            CodeType = _nikke.GameData?.CodeType ?? eNikkeCode.None;

            if (_nikke.Weapon != null)
            {
                MaxAmmo.Value = _nikke.Weapon.MaxAmmo;
                CurrentAmmo.Value = _nikke.Weapon.CurrentAmmo.Value;
                _nikke.Weapon.CurrentAmmo.OnValueChanged += OnAmmoChanged;
            }

            // 리소스 로드 (비동기)
            _ = LoadResourcesAsync();
        }
    }

    private void UpdateHp(long current, long max)
    {
        float ratio = max > 0 ? (float)current / max : 0f;
        HpRatio.Value = ratio;
    }

    private void OnSelectedChanged(bool isSelected)
    {
        IsSelected.Value = isSelected;
    }

    private void OnStateChanged(eNikkeState state)
    {
        CurrentState.Value = state;
    }

    private void OnAmmoChanged(int current)
    {
        CurrentAmmo.Value = current;
    }

    private async Task LoadResourcesAsync()
    {
        if (_nikke == null || _nikke.GameData == null) return;

        string name = _nikke.GameData.name;

        // 1. 크롭 이미지 로드
        string cropPath = $"Assets/Textures/Nikke/{name}_Crop";
        ProfileImage.Value = await Managers.Resource.LoadAsync<Sprite>(cropPath);

        // 2. 속성 코드 아이콘 로드
        string codePath = $"Assets/Textures/Icon/Code/{_nikke.GameData.element}";
        CodeIcon.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);
    }

    protected override void OnDispose()
    {
        if (_nikke != null)
        {
            _nikke.OnHpChanged -= UpdateHp;
            _nikke.State.OnValueChanged -= OnStateChanged;
            _nikke.IsSelected.OnValueChanged -= OnSelectedChanged;

            if (_nikke.Weapon != null)
            {
                _nikke.Weapon.CurrentAmmo.OnValueChanged -= OnAmmoChanged;
            }
        }
        base.OnDispose();
    }
}
