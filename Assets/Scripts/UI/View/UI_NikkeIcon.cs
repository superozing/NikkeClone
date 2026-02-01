using System;
using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Enum removed


public class UI_NikkeIcon : UI_View, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    // --- Components ---
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
    // --- Drag & Drop State ---
    private int _slotIndex;
    public int SlotIndex => _slotIndex;

    private RectTransform _dragLayer;
    private Transform _originalParent;
    private GameObject _emptyImageRef; // 내 슬롯의 Empty Image

    // --- Interaction Events (Removed) ---
    // Actions are moved to ViewModel

    // --- Interaction State ---
    private bool _isPointerDown = false;
    private bool _isDragging = false;
    private bool _isLongPressTriggered = false;
    private Coroutine _longPressCoroutine;
    private const float LONG_PRESS_DURATION = 0.5f;


    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 외부(Popup)에서 초기 설정 값을 주입합니다.
    /// </summary>
    /// <summary>
    /// 외부(Popup)에서 초기 설정 값을 주입합니다.
    /// </summary>
    public void Initialize(
        int slotIndex,
        RectTransform dragLayer,
        GameObject emptyImage)
    {
        _slotIndex = slotIndex;
        _dragLayer = dragLayer;
        _emptyImageRef = emptyImage;
    }


    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as NikkeIconViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        _canvasGroup.blocksRaycasts = true;

        // ReactiveProperty 바인딩
        Bind(_viewModel.FaceSprite, SetSprite(_faceImage));
        Bind(_viewModel.BurstIcon, SetSprite(_burstIcon));
        Bind(_viewModel.CodeIcon, SetSprite(_codeIcon));
        Bind(_viewModel.WeaponIcon, SetSprite(_weaponIcon));
        Bind(_viewModel.LevelText, text => _levelText.text = text);
        Bind(_viewModel.RarityColor, color =>
        {
            if (_rarityFrameImage != null) _rarityFrameImage.color = color;
        });
        Bind(_viewModel.IsEmpty, isEmpty => _contentRoot.SetActive(!isEmpty));
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

    // --- Pointer Input & Long Press Logic ---

    public void OnPointerDown(PointerEventData eventData)
    {
        // 데이터가 없으면 반응하지 않음
        if (_viewModel == null || _viewModel.IsEmpty.Value) return;

        _isPointerDown = true;
        _isDragging = false;
        _isLongPressTriggered = false;

        // 롱프레스 체크 시작
        if (_longPressCoroutine != null) StopCoroutine(_longPressCoroutine);
        _longPressCoroutine = StartCoroutine(CheckLongPress());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isPointerDown) return;

        _isPointerDown = false;
        if (_longPressCoroutine != null) StopCoroutine(_longPressCoroutine);

        // 드래그도 안했고, 롱프레스도 아니었다면 -> 클릭
        if (!_isDragging && !_isLongPressTriggered)
        {
            // 단순 클릭 시 처리 (ViewModel에 위임)
            _viewModel?.OnClick();
        }

    }

    private IEnumerator CheckLongPress()
    {
        yield return new WaitForSeconds(LONG_PRESS_DURATION);

        // 여전히 눌려있고 드래그 중이 아니라면 롱프레스 인정
        if (_isPointerDown && !_isDragging)
        {
            _isLongPressTriggered = true;
            Debug.Log($"[UI_NikkeIcon] Slot {_slotIndex} Long Press Triggered -> ViewModel HandleLongPress");

            // 롱프레스 시 처리 (ViewModel에 위임)
            _viewModel?.OnLongPress();

        }
    }

    // --- Drag Implementation ---

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 데이터가 없거나 드래그 불가능하면 리턴
        if (_viewModel == null || _viewModel.IsEmpty.Value || !_viewModel.IsDraggable) return;


        // 롱프레스 취소 처리
        _isDragging = true;
        if (_longPressCoroutine != null) StopCoroutine(_longPressCoroutine);

        _originalParent = transform.parent;

        // 1. 드래그 레이어로 이동 (맨 위로)
        if (_dragLayer != null)
            transform.SetParent(_dragLayer);

        // 2. 원래 자리에 Empty Image 활성화
        if (_emptyImageRef != null)
            _emptyImageRef.SetActive(true);

        // 3. 레이캐스트 차단 해제 (Drop 및 밑에 있는 아이콘 Highlight 감지를 위해)
        _canvasGroup.blocksRaycasts = false;

        // 4. 내 자신 하이라이트 (선택됨 느낌)
        SetHighlight(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // IsDraggable 체크는 OnBeginDrag에서 수행됨. _isDragging flag가 false면 실행 안됨.

        if (!_isDragging) return;

        RectTransform plane = _dragLayer != null ? _dragLayer : (transform.parent as RectTransform);
        if (plane != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(plane, eventData.position, eventData.pressEventCamera, out Vector3 worldPos))
        {
            transform.position = worldPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {

        if (!_isDragging) return;

        _isDragging = false;
        _canvasGroup.blocksRaycasts = true;
        SetHighlight(false);

        // 드롭 로직에 의해 부모가 바뀌지 않았다면(Swap 실패 혹은 빈 공간 드롭) 원래 자리로 복귀
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

    // --- Drop & Highlight Logic ---

    /// <summary>
    /// 다른 아이콘이 내 위에 드롭되었을 때 호출됩니다. (데이터 교환)
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        var droppedIcon = eventData.pointerDrag?.GetComponent<UI_NikkeIcon>();
        if (droppedIcon != null && droppedIcon != this)
        {
            Debug.Log($"Slot {droppedIcon._slotIndex} dropped on Slot {this._slotIndex}");
            _viewModel?.OnDrop(droppedIcon.SlotIndex);

            // 드롭 받은 즉시 하이라이트 끄기
            SetHighlight(false);
        }
    }


    /// <summary>
    /// 드래그 중인 포인터가 내 영역에 들어왔을 때 (하이라이트 켜기)
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중인 대상이 UI_NikkeIcon이어야 함
        if (eventData.dragging && eventData.pointerDrag != null)
        {
            var draggingIcon = eventData.pointerDrag.GetComponent<UI_NikkeIcon>();
            if (draggingIcon != null && draggingIcon != this)
            {
                SetHighlight(true);
            }
        }
    }

    /// <summary>
    /// 드래그 중인 포인터가 내 영역을 나갔을 때 (하이라이트 끄기)
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        // 드래그 중일 때만 반응
        if (eventData.dragging)

        {
            SetHighlight(false);
        }
    }

    /// <summary>
    /// 하이라이트 이미지(호버/선택 효과)의 활성 상태를 설정합니다.
    /// </summary>
    public void SetHighlight(bool active)
    {
        if (_highlightImage != null)
            _highlightImage.gameObject.SetActive(active);
    }
}