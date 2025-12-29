using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillSlot : UI_View
{
    [Header("Texts")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descText;
    [SerializeField] private TMP_Text _typeText;     // 패시브/액티브
    [SerializeField] private TMP_Text _cooldownText; // 쿨타임 시간

    [Header("Images")]
    [SerializeField] private Image _skillIconImage;

    [Header("Layout Groups")]
    [SerializeField] private GameObject _cooldownGroup; // 쿨타임 아이콘 + 텍스트 그룹

    private SkillSlotViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as SkillSlotViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 텍스트 바인딩
        Bind(_viewModel.Name, text => SetText(_nameText, text));
        Bind(_viewModel.Description, text => SetText(_descText, text));
        Bind(_viewModel.SkillType, text => SetText(_typeText, text));
        Bind(_viewModel.CooldownText, text => SetText(_cooldownText, text));

        // 이미지 바인딩
        Bind(_viewModel.SkillIcon, SetSprite);

        // 활성/비활성 바인딩
        Bind(_viewModel.IsCooldownVisible, visible =>
        {
            if (_cooldownGroup != null)
                _cooldownGroup.SetActive(visible);
        });
    }

    private void SetText(TMP_Text target, string text)
    {
        if (target != null)
            target.text = text;
    }

    private void SetSprite(Sprite sprite)
    {
        if (_skillIconImage == null) return;

        bool isValid = sprite != null;
        _skillIconImage.gameObject.SetActive(isValid);

        if (isValid)
            _skillIconImage.sprite = sprite;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _viewModel = null;
    }
}