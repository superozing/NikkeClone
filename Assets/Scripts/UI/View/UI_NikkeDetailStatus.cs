using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_NikkeDetailStatus : UI_View
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private Image _rarityImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Image _burstIcon;
    [SerializeField] private TMP_Text _combatPowerText;

    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _attackText;
    [SerializeField] private TMP_Text _defenseText;

    [SerializeField] private Button _levelUpButton;

    [SerializeField] private TMP_Text _squadText;
    [SerializeField] private Image _codeIcon;     // 속성
    [SerializeField] private Image _weaponIcon;   // 무기
    [SerializeField] private Image _classIcon;    // 클래스
    [SerializeField] private Image _manufacturerIcon; // 기업

    [SerializeField] private Button[] _skillButtons; // 스킬 버튼 3개

    private NikkeDetailStatusViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        if (_levelUpButton != null)
            _levelUpButton.onClick.AddListener(OnLevelUpClick);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 이전 뷰모델 연결 해제
        if (_viewModel != null)
        {
            _viewModel.OnRequestLevelUpPopup -= ShowLevelUpPopup;
            _viewModel.OnRequestSkillInfoPopup -= ShowSkillInfoPopup;
        }

        // 버튼 리스너 해제
        foreach (var btn in _skillButtons)
            btn.onClick.RemoveListener(OnSkillButtonClick);

        _viewModel = viewModel as NikkeDetailStatusViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 이벤트 구독
        _viewModel.OnRequestLevelUpPopup += ShowLevelUpPopup;
        _viewModel.OnRequestSkillInfoPopup += ShowSkillInfoPopup;

        // 버튼 리스너 등록
        foreach (var btn in _skillButtons)
            btn.onClick.AddListener(OnSkillButtonClick);

        // 텍스트 바인딩
        Bind(_viewModel.LevelText, text => SetText(_levelText, text));
        Bind(_viewModel.Name, text => SetText(_nameText, text));
        Bind(_viewModel.CombatPower, text => SetText(_combatPowerText, text));
        Bind(_viewModel.HP, text => SetText(_hpText, text));
        Bind(_viewModel.Attack, text => SetText(_attackText, text));
        Bind(_viewModel.Defense, text => SetText(_defenseText, text));
        Bind(_viewModel.Squad, text => SetText(_squadText, text));

        // 이미지 바인딩
        Bind(_viewModel.RarityIcon, sprite => SetSprite(_rarityImage, sprite));
        Bind(_viewModel.BurstIcon, sprite => SetSprite(_burstIcon, sprite));
        Bind(_viewModel.CodeIcon, sprite => SetSprite(_codeIcon, sprite));
        Bind(_viewModel.ClassIcon, sprite => SetSprite(_classIcon, sprite));
        Bind(_viewModel.WeaponIcon, sprite => SetSprite(_weaponIcon, sprite));
        Bind(_viewModel.ManufacturerIcon, sprite => SetSprite(_manufacturerIcon, sprite));
    }

    private void OnLevelUpClick() => _viewModel?.OnClickLevelUp();
    private void OnSkillButtonClick() => _viewModel?.OnClickSkill();

    /// <summary>
    /// 뷰모델의 요청에 따라 레벨업 팝업을 띄웁니다.
    /// </summary>
    private async void ShowLevelUpPopup(int nikkeId)
    {
        NikkeLevelUpPopupViewModel popupVM = new NikkeLevelUpPopupViewModel();
        popupVM.SetNikke(nikkeId);

        await Managers.UI.ShowAsync<UI_NikkeLevelUpPopup>(popupVM);
    }

    /// <summary>
    /// [추가] 뷰모델의 요청에 따라 스킬 정보 팝업을 띄웁니다.
    /// </summary>
    private async void ShowSkillInfoPopup(int nikkeId)
    {
        SkillInfoPopupViewModel popupVM = new SkillInfoPopupViewModel();
        popupVM.SetData(nikkeId);

        await Managers.UI.ShowAsync<UI_SkillInfoPopup>(popupVM);
    }

    // --- Helper Methods ---
    private void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text;
    }

    private void SetSprite(Image target, Sprite sprite)
    {
        if (target == null) return;

        // 스프라이트가 null이면 비활성화하여 임시 처리
        bool hasSprite = sprite != null;
        target.gameObject.SetActive(hasSprite);

        if (hasSprite)
            target.sprite = sprite;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_levelUpButton != null)
            _levelUpButton.onClick.RemoveListener(OnLevelUpClick);

        // 버튼 리스너 해제
        foreach (var btn in _skillButtons)
            btn.onClick.RemoveListener(OnSkillButtonClick);

        if (_viewModel != null)
        {
            _viewModel.OnRequestLevelUpPopup -= ShowLevelUpPopup;
            _viewModel.OnRequestSkillInfoPopup -= ShowSkillInfoPopup;
        }

        _viewModel = null;
    }
}