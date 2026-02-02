using System;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_NikkeCardSortFilter : UI_Popup, IUIShowHideable
{
    public override string ActionMapKey => "UI_NikkeCardSortFilter";

    [Header("Sort Buttons")]
    [SerializeField] private Button _sortByPowerButton;
    [SerializeField] private Button _sortByLevelButton;

    [Header("Filter Buttons - Class")]
    [SerializeField] private Button _classAttackerButton;
    [SerializeField] private Button _classDefenderButton;
    [SerializeField] private Button _classSupporterButton;

    [Header("Filter Buttons - Code")]
    [SerializeField] private Button _codeFireButton;
    [SerializeField] private Button _codeWaterButton;
    [SerializeField] private Button _codeWindButton;
    [SerializeField] private Button _codeElectricButton;
    [SerializeField] private Button _codeIronButton;

    [Header("Filter Buttons - Weapon")]
    [SerializeField] private Button _weaponARButton;
    [SerializeField] private Button _weaponSMGButton;
    [SerializeField] private Button _weaponSGButton;
    [SerializeField] private Button _weaponSRButton;
    [SerializeField] private Button _weaponRLButton;
    [SerializeField] private Button _weaponMGButton;

    [Header("Filter Buttons - Manufacturer")]
    [SerializeField] private Button _manufElysionButton;
    [SerializeField] private Button _manufMissilisButton;
    [SerializeField] private Button _manufTetraButton;
    [SerializeField] private Button _manufPilgrimButton;
    [SerializeField] private Button _manufAbnormalButton;

    [Header("Common")]
    [SerializeField] private Button _blocker;

    // 활성/비활성 상태 색상 정의
    private readonly Color _activeColor = new Color(0.2f, 0.7f, 0.9f);
    private readonly Color _inactiveColor = new Color(0.2f, 0.2f, 0.2f);

    private INikkeCardScrollViewModel _viewModel;
    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();
        _showAnim = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
        _hideAnim = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);
        // 팝업이므로 ESC 키 바인딩 (ViewModel에 닫기 요청)
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 부모 SetViewModel 호출
        // 내부적으로 UnbindAll()이 호출되어 _disposables(ReactiveProperty 구독)가 해제됩니다.
        base.SetViewModel(viewModel);

        _viewModel = viewModel as INikkeCardScrollViewModel;

        if (_viewModel == null) return;

        // 2. 모든 리스너 및 데이터 바인딩 재설정
        BindAll();
    }
    private void BindAll()
    {
        _blocker.onClick.RemoveAllListeners();
        _blocker.onClick.AddListener(() => _viewModel.RequestCloseSortFilter());

        _sortByPowerButton.onClick.RemoveAllListeners();
        _sortByPowerButton.onClick.AddListener(() => _viewModel.SetSortType(eNikkeSortType.CombatPower));
        _sortByLevelButton.onClick.RemoveAllListeners();
        _sortByLevelButton.onClick.AddListener(() => _viewModel.SetSortType(eNikkeSortType.Level));

        Bind(_viewModel.SortType, UpdateSortButtons);

        // 클래스
        BindFilter(_classAttackerButton, eNikkeClass.Attacker, _viewModel.ClassFilters, _viewModel.ToggleClassFilter);
        BindFilter(_classDefenderButton, eNikkeClass.Defender, _viewModel.ClassFilters, _viewModel.ToggleClassFilter);
        BindFilter(_classSupporterButton, eNikkeClass.Supporter, _viewModel.ClassFilters, _viewModel.ToggleClassFilter);

        // 속성코드
        BindFilter(_codeFireButton, eNikkeCode.Fire, _viewModel.CodeFilters, _viewModel.ToggleCodeFilter);
        BindFilter(_codeWaterButton, eNikkeCode.Water, _viewModel.CodeFilters, _viewModel.ToggleCodeFilter);
        BindFilter(_codeWindButton, eNikkeCode.Wind, _viewModel.CodeFilters, _viewModel.ToggleCodeFilter);
        BindFilter(_codeElectricButton, eNikkeCode.Electric, _viewModel.CodeFilters, _viewModel.ToggleCodeFilter);
        BindFilter(_codeIronButton, eNikkeCode.Iron, _viewModel.CodeFilters, _viewModel.ToggleCodeFilter);

        // 무기
        BindFilter(_weaponARButton, eNikkeWeapon.AR, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);
        BindFilter(_weaponSMGButton, eNikkeWeapon.SMG, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);
        BindFilter(_weaponSGButton, eNikkeWeapon.SG, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);
        BindFilter(_weaponSRButton, eNikkeWeapon.SR, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);
        BindFilter(_weaponRLButton, eNikkeWeapon.RL, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);
        BindFilter(_weaponMGButton, eNikkeWeapon.MG, _viewModel.WeaponFilters, _viewModel.ToggleWeaponFilter);

        // 기업
        BindFilter(_manufElysionButton, eNikkeManufacturer.Elysion, _viewModel.ManufacturerFilters, _viewModel.ToggleManufacturerFilter);
        BindFilter(_manufMissilisButton, eNikkeManufacturer.Missilis, _viewModel.ManufacturerFilters, _viewModel.ToggleManufacturerFilter);
        BindFilter(_manufTetraButton, eNikkeManufacturer.Tetra, _viewModel.ManufacturerFilters, _viewModel.ToggleManufacturerFilter);
        BindFilter(_manufPilgrimButton, eNikkeManufacturer.Pilgrim, _viewModel.ManufacturerFilters, _viewModel.ToggleManufacturerFilter);
        BindFilter(_manufAbnormalButton, eNikkeManufacturer.Abnormal, _viewModel.ManufacturerFilters, _viewModel.ToggleManufacturerFilter);
    }

    /// <summary>
    /// 바인딩 로직 중복 제거용 내부 헬퍼
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="btn"></param>
    /// <param name="typeValue"></param>
    /// <param name="filterArray"></param>
    private void BindFilter<T>(Button btn, T typeValue, ReactiveProperty<bool>[] filterArray, Action<T> toggleAction) where T : Enum
    {
        if (btn == null) return;
        int index = Convert.ToInt32(typeValue);

        if (index < filterArray.Length)
        {
            // Event
            btn.onClick.RemoveAllListeners();
            // 클릭 시 ViewModel의 배열 값을 직접 토글 (ViewModel 설계에 따름)
            btn.onClick.AddListener(() => toggleAction?.Invoke(typeValue));

            // Binding
            Bind(filterArray[index], isActive => UpdateButtonColor(btn, isActive));
        }
    }

    private void UpdateButtonColor(Button button, bool isActive)
    {
        if (button != null && button.image != null)
            button.image.color = isActive ? _activeColor : _inactiveColor;
    }

    private void UpdateSortButtons(eNikkeSortType sortType)
    {
        // 선택된 정렬 버튼 비활성화 (현재 선택됨을 표시)
        if (_sortByPowerButton) _sortByPowerButton.interactable = (sortType != eNikkeSortType.CombatPower);
        if (_sortByLevelButton) _sortByLevelButton.interactable = (sortType != eNikkeSortType.Level);
    }

    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }
    // --- IUIShowHideable Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_showAnim != null)
            await _showAnim.ExecuteAsync(delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_hideAnim != null)
            await _hideAnim.ExecuteAsync();
    }

    /// <summary>
    /// 외부(ScrollView)에서 애니메이션 종료를 기다릴 수 있도록 래퍼 메서드 제공
    /// </summary>
    public async Task CloseAsync()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    private void OnEscapeAction(InputAction.CallbackContext context) => _viewModel?.RequestCloseSortFilter();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);
        _viewModel = null;
    }
}