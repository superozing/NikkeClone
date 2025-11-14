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

    private IIconViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        // 버튼 클릭 이벤트 바인딩
        _clickButton.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick() => _viewModel?.OnClickButton();

    public override void SetViewModel(IViewModel viewModel)
    {
        if (viewModel != null && viewModel is not IIconViewModel)
        {
            Debug.LogError($"[UI_Icon] 잘못된 ViewModel 타입이 주입되었습니다. Expected: {typeof(IIconViewModel).Name}, Actual: {viewModel.GetType().Name}");
            return;
        }

        _viewModel = viewModel as IIconViewModel;

        base.SetViewModel(_viewModel);
    }

    protected override void OnStateChanged()
    {
        // 1. 수량 텍스트 갱신
        bool showQuantity = !string.IsNullOrEmpty(_viewModel.QuantityText);
        _quantityRoot.SetActive(showQuantity);
        if (showQuantity)
            _quantityText.text = _viewModel.QuantityText;

        // 2. 스프라이트 갱신
        bool hasIcon = _viewModel.MainIconSprite != null;
        _iconImage.gameObject.SetActive(hasIcon);
        if (hasIcon)
            _iconImage.sprite = _viewModel.MainIconSprite;

        bool hasFrame = _viewModel.RarityFrameSprite != null;
        _rarityFrameImage.gameObject.SetActive(hasFrame);
        if (hasFrame)
            _rarityFrameImage.sprite = _viewModel.RarityFrameSprite;
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