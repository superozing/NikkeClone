using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_NikkeIcon : UI_View, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("Components")]
    [SerializeField] private GameObject _contentRoot; // 데이터 있을 때 표시할 루트
    [SerializeField] private Image _faceImage;
    [SerializeField] private Image _highlightImage; // 호버 또는 드래그 시 강조
    [SerializeField] private Image _rarityFrameImage; // 등급 프레임 (배경 등)

    [Header("Info Group")]
    [SerializeField] private Image _burstIcon;
    [SerializeField] private Image _codeIcon;
    [SerializeField] private Image _weaponIcon;
    [SerializeField] private TMP_Text _levelText;

    private NikkeIconViewModel _viewModel;
    private RectTransform _rectTransform;

    // --- Drag & Drop State ---
    private int _slotIndex;
    private RectTransform _dragLayer;
    private Transform _originalParent;
    private GameObject _emptyImageRef; // 내 슬롯의 Empty Image
    private Action<int, int> _onSwapRequest;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 외부(Popup)에서 초기 설정 값을 주입합니다.
    /// </summary>
    public void Initialize(int slotIndex, RectTransform dragLayer, Action<int, int> onSwap, GameObject emptyImage)
    {
        _slotIndex = slotIndex;
        _dragLayer = dragLayer;
        _onSwapRequest = onSwap;
        _emptyImageRef = emptyImage;
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as NikkeIconViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 데이터 존재 여부에 따라 Content 활성/비활성
        bool hasData = _viewModel.HasData;
        _contentRoot.SetActive(hasData);

        // 빈 슬롯이면 상호작용(드래그) 차단
        _canvasGroup.blocksRaycasts = hasData;

        if (hasData)
        {
            Bind(_viewModel.FaceSprite, SetSprite(_faceImage));
            Bind(_viewModel.BurstIcon, SetSprite(_burstIcon));
            Bind(_viewModel.CodeIcon, SetSprite(_codeIcon));
            Bind(_viewModel.WeaponIcon, SetSprite(_weaponIcon));
            Bind(_viewModel.LevelText, text => _levelText.text = text);
            Bind(_viewModel.RarityColor, color =>
            {
                if (_rarityFrameImage != null) _rarityFrameImage.color = color;
            });
        }
    }

    private Action<Sprite> SetSprite(Image target)
    {
        return (sprite) =>
        {
            if (target == null) return;
            target.sprite = sprite;
            target.gameObject.SetActive(sprite != null);
        };
    }

    // --- Drag Implementation ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_viewModel.HasData) return;

        _originalParent = transform.parent;

        // 1. 드래그 레이어로 이동 (맨 위로)
        if (_dragLayer != null)
            transform.SetParent(_dragLayer);

        // 2. 원래 자리에 Empty Image 활성화
        if (_emptyImageRef != null)
            _emptyImageRef.SetActive(true);

        // 3. 레이캐스트 차단 해제 (Drop 감지를 위해)
        _canvasGroup.blocksRaycasts = false;

        // 4. 하이라이트 표시
        if (_highlightImage != null) _highlightImage.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_viewModel.HasData) return;

        // 마우스 따라다니기
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_viewModel.HasData) return;

        // 1. 레이캐스트 복구
        _canvasGroup.blocksRaycasts = true;

        // 2. 하이라이트 끄기
        if (_highlightImage != null) _highlightImage.gameObject.SetActive(false);

        // 3. 드롭 로직에 의해 부모가 바뀌지 않았다면 원래 자리로 복귀
        // (OnDrop이 성공하면 ViewModel 갱신에 의해 UI가 다시 그려지므로 여기서는 실패 시 복구만 담당)
        if (transform.parent == _dragLayer)
        {
            ReturnToOriginalSlot();
        }
    }

    /// <summary>
    /// 드래그 실패 혹은 취소 시 호출
    /// </summary>
    private void ReturnToOriginalSlot()
    {
        transform.SetParent(_originalParent);
        _rectTransform.anchoredPosition = Vector2.zero;

        if (_emptyImageRef != null)
            _emptyImageRef.SetActive(false);
    }

    /// <summary>
    /// 다른 아이콘이 내 위에 드롭되었을 때 호출됩니다.
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var droppedIcon = eventData.pointerDrag?.GetComponent<UI_NikkeIcon>();
        if (droppedIcon != null && droppedIcon != this)
        {
            // Swap 요청
            _onSwapRequest?.Invoke(droppedIcon._slotIndex, this._slotIndex);
        }
    }
}