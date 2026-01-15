using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_Icon : UI_View
{
    [Header("Components")]
    [SerializeField] private Image _iconImage;
    [SerializeField] private Image _rarityFrameImage;

    [Header("Quantity")]
    [SerializeField] private GameObject _quantityRoot;
    [SerializeField] private TMP_Text _quantityText;

    [Header("Button")]
    [SerializeField] private Button _clickButton;

    private IconViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 클릭 이벤트 바인딩
        _clickButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick() => _viewModel?.OnClickButton();

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as IconViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // ReactiveProperty 바인딩
        Bind(_viewModel.MainIconSprite, UpdateMainIcon);
        Bind(_viewModel.RarityFrameSprite, UpdateFrameIcon);
        Bind(_viewModel.QuantityText, UpdateQuantity);
    }

    private void UpdateMainIcon(Sprite sprite)
    {
        bool hasIcon = sprite != null;
        _iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon)
            _iconImage.sprite = sprite;
    }

    private void UpdateFrameIcon(Sprite sprite)
    {
        bool hasFrame = sprite != null;
        _rarityFrameImage.gameObject.SetActive(hasFrame);
        if (hasFrame)
            _rarityFrameImage.sprite = sprite;
    }

    private void UpdateQuantity(string text)
    {
        bool showQuantity = !string.IsNullOrEmpty(text);
        _quantityRoot.SetActive(showQuantity);
        if (showQuantity)
            _quantityText.text = text;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_clickButton != null)
            _clickButton.onClick.RemoveListener(OnButtonClick);

        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}