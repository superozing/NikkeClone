using System;
using UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class UI_NikkeCardSortFilter : UI_View, IUIShowHideAnimation
{
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
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _blocker;

    // 활성/비활성 상태 색상 정의
    private readonly Color _activeColor = new Color(0.2f, 0.7f, 0.9f);
    private readonly Color _inactiveColor = new Color(0.2f, 0.2f, 0.2f);

    private NikkeCardScrollViewModel _viewModel;
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 부모 SetViewModel 호출
        // 내부적으로 UnbindAll()이 호출되어 _disposables(ReactiveProperty 구독)가 해제됩니다.
        base.SetViewModel(viewModel);

        _viewModel = viewModel as NikkeCardScrollViewModel;

        if (_viewModel == null) return;

        // 2. 모든 리스너 및 데이터 바인딩 재설정
        BindAll();
    }

    private void BindAll()
    {
        _closeButton.onClick.RemoveAllListeners();
        _closeButton.onClick.AddListener(OnClose);
        _blocker.onClick.RemoveAllListeners();
        _blocker.onClick.AddListener(OnClose);

        _sortByPowerButton.onClick.RemoveAllListeners();
        _sortByPowerButton.onClick.AddListener(() => _viewModel.SetSortType(eNikkeSortType.CombatPower));
        _sortByLevelButton.onClick.RemoveAllListeners();
        _sortByLevelButton.onClick.AddListener(() => _viewModel.SetSortType(eNikkeSortType.Level));

        Bind(_viewModel.SortType, UpdateSortButtons);

        // -------------------------------------------------------------------------
        // 클래스 필터
        // -------------------------------------------------------------------------

        // 화력형
        if (_classAttackerButton != null)
        {
            var prop = _viewModel.ClassFilters[(int)eNikkeClass.Attacker];
            _classAttackerButton.onClick.RemoveAllListeners();
            _classAttackerButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_classAttackerButton, isActive));
        }

        // 방어형
        if (_classDefenderButton != null)
        {
            var prop = _viewModel.ClassFilters[(int)eNikkeClass.Defender];
            _classDefenderButton.onClick.RemoveAllListeners();
            _classDefenderButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_classDefenderButton, isActive));
        }

        // 지원형
        if (_classSupporterButton != null)
        {
            var prop = _viewModel.ClassFilters[(int)eNikkeClass.Supporter];
            _classSupporterButton.onClick.RemoveAllListeners();
            _classSupporterButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_classSupporterButton, isActive));
        }

        // -------------------------------------------------------------------------
        // 코드(속성) 필터
        // -------------------------------------------------------------------------

        // 작열
        if (_codeFireButton != null)
        {
            var prop = _viewModel.CodeFilters[(int)eNikkeCode.Fire];
            _codeFireButton.onClick.RemoveAllListeners();
            _codeFireButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_codeFireButton, isActive));
        }

        // 수냉
        if (_codeWaterButton != null)
        {
            var prop = _viewModel.CodeFilters[(int)eNikkeCode.Water];
            _codeWaterButton.onClick.RemoveAllListeners();
            _codeWaterButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_codeWaterButton, isActive));
        }

        // 풍압
        if (_codeWindButton != null)
        {
            var prop = _viewModel.CodeFilters[(int)eNikkeCode.Wind];
            _codeWindButton.onClick.RemoveAllListeners();
            _codeWindButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_codeWindButton, isActive));
        }

        // 전격
        if (_codeElectricButton != null)
        {
            var prop = _viewModel.CodeFilters[(int)eNikkeCode.Electric];
            _codeElectricButton.onClick.RemoveAllListeners();
            _codeElectricButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_codeElectricButton, isActive));
        }

        // 철갑
        if (_codeIronButton != null)
        {
            var prop = _viewModel.CodeFilters[(int)eNikkeCode.Iron];
            _codeIronButton.onClick.RemoveAllListeners();
            _codeIronButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_codeIronButton, isActive));
        }

        // -------------------------------------------------------------------------
        // 무기 필터
        // -------------------------------------------------------------------------

        // AR (소총)
        if (_weaponARButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.AR];
            _weaponARButton.onClick.RemoveAllListeners();
            _weaponARButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponARButton, isActive));
        }

        // SMG (기관단총)
        if (_weaponSMGButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.SMG];
            _weaponSMGButton.onClick.RemoveAllListeners();
            _weaponSMGButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponSMGButton, isActive));
        }

        // SG (산탄총)
        if (_weaponSGButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.SG];
            _weaponSGButton.onClick.RemoveAllListeners();
            _weaponSGButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponSGButton, isActive));
        }

        // SR (저격소총)
        if (_weaponSRButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.SR];
            _weaponSRButton.onClick.RemoveAllListeners();
            _weaponSRButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponSRButton, isActive));
        }

        // RL (런처)
        if (_weaponRLButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.RL];
            _weaponRLButton.onClick.RemoveAllListeners();
            _weaponRLButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponRLButton, isActive));
        }

        // MG (기관총)
        if (_weaponMGButton != null)
        {
            var prop = _viewModel.WeaponFilters[(int)eNikkeWeapon.MG];
            _weaponMGButton.onClick.RemoveAllListeners();
            _weaponMGButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_weaponMGButton, isActive));
        }

        // -------------------------------------------------------------------------
        // 기업 필터
        // -------------------------------------------------------------------------

        // 엘리시온
        if (_manufElysionButton != null)
        {
            var prop = _viewModel.ManufacturerFilters[(int)eNikkeManufacturer.Elysion];
            _manufElysionButton.onClick.RemoveAllListeners();
            _manufElysionButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_manufElysionButton, isActive));
        }

        // 미실리스
        if (_manufMissilisButton != null)
        {
            var prop = _viewModel.ManufacturerFilters[(int)eNikkeManufacturer.Missilis];
            _manufMissilisButton.onClick.RemoveAllListeners();
            _manufMissilisButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_manufMissilisButton, isActive));
        }

        // 테트라
        if (_manufTetraButton != null)
        {
            var prop = _viewModel.ManufacturerFilters[(int)eNikkeManufacturer.Tetra];
            _manufTetraButton.onClick.RemoveAllListeners();
            _manufTetraButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_manufTetraButton, isActive));
        }

        // 필그림
        if (_manufPilgrimButton != null)
        {
            var prop = _viewModel.ManufacturerFilters[(int)eNikkeManufacturer.Pilgrim];
            _manufPilgrimButton.onClick.RemoveAllListeners();
            _manufPilgrimButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_manufPilgrimButton, isActive));
        }

        // 어브노멀
        if (_manufAbnormalButton != null)
        {
            var prop = _viewModel.ManufacturerFilters[(int)eNikkeManufacturer.Abnormal];
            _manufAbnormalButton.onClick.RemoveAllListeners();
            _manufAbnormalButton.onClick.AddListener(() => prop.Value = !prop.Value);
            Bind(prop, isActive => UpdateButtonColor(_manufAbnormalButton, isActive));
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

    // --- IUIShowHideAnimation Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_fadeIn != null && _canvasGroup != null)
            await _fadeIn.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_fadeOut != null && _canvasGroup != null)
            await _fadeOut.ExecuteAsync(_canvasGroup, delay);
    }

    /// <summary>
    /// 외부(ScrollView)에서 애니메이션 종료를 기다릴 수 있도록 래퍼 메서드 제공
    /// </summary>
    public async Task CloseAsync()
    {
        await PlayHideAnimationAsync();
        gameObject.SetActive(false);
    }
    private void OnClose() => _viewModel?.RequestCloseSortFilter();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _viewModel = null;
    }
}