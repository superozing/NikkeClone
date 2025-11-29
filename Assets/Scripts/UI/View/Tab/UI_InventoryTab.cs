using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Inventory;

    [SerializeField] private GameObject _iconTemplate;
    private List<UI_Icon> _spawnedIcons = new List<UI_Icon>();

    [SerializeField] private RectTransform _itemBg;
    [SerializeField] private CanvasGroup _scrollCanvasGroup;
    [SerializeField] private Transform _contentTransform;

    private InventoryTabViewModel _viewModel;

    private IUIAnimation _openAnimation;

    protected override void Awake()
    {
        base.Awake();
        _iconTemplate.SetActive(false);

        
        _openAnimation = new InventoryOpenAnimation(_itemBg, 0.3f);

        _itemBg.localScale = new Vector3(0f, 1f, 1f); // x축 커지며 확장시키기 위해 0으로 설정
        _scrollCanvasGroup.alpha = 0f; 
        _scrollCanvasGroup.interactable = false;
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 기존 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnRequestItemDetail -= ShowItemDetailPopup;
            _viewModel.OnInventoryUpdated -= RefreshList;
        }

        _viewModel = viewModel as InventoryTabViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // 2. 새 이벤트 구독
            _viewModel.OnRequestItemDetail += ShowItemDetailPopup;
            _viewModel.OnInventoryUpdated += RefreshList;

            // 이미 데이터 로드가 끝난 경우를 체크하기 위해 호출
            if (_viewModel.ItemViewModels.Count > 0)
                RefreshList();
        }
    }

    private async void ShowItemDetailPopup(eItemType itemType)
    {
        ItemDetailPopupViewModel popupVM = new ItemDetailPopupViewModel();
        await popupVM.SetItem(itemType);
        await Managers.UI.ShowAsync<UI_ItemDetailPopup>(popupVM);
    }

    public override void OnTabSelected()
    {
        base.OnTabSelected();
        PlayShowAnimation();
    }

    public override void OnTabDeselected()
    {
        base.OnTabDeselected();

        _itemBg.localScale = new Vector3(0f, 1f, 1f);
        _scrollCanvasGroup.alpha = 0f;
    }

    private async void PlayShowAnimation() => await _openAnimation.ExecuteAsync(_scrollCanvasGroup);

    private void RefreshList()
    {
        if (_iconTemplate == null || _contentTransform == null) 
            return;

        // 기존 아이콘 제거
        foreach (var icon in _spawnedIcons)
            Managers.Resource.Destroy(icon.gameObject);

        _spawnedIcons.Clear();

        foreach (var itemVM in _viewModel.ItemViewModels)
        {
            GameObject go = Instantiate(_iconTemplate, _contentTransform);
            go.SetActive(true);

            UI_Icon uiIcon = go.GetComponent<UI_Icon>();
            if (uiIcon != null)
            {
                uiIcon.SetViewModel(itemVM);
                _spawnedIcons.Add(uiIcon);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_viewModel != null)
        {
            _viewModel.OnRequestItemDetail -= ShowItemDetailPopup;
            _viewModel.OnInventoryUpdated -= RefreshList;
        }
    }
}