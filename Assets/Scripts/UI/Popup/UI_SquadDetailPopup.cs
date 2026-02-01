using System;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_SquadDetailPopup : UI_Popup, IUIShowHideAnimation
{
    public override string ActionMapKey => "UI_SquadDetailPopup";

    [Header("Top Area")]
    [SerializeField] private TMP_Text _combatPowerText;
    [SerializeField] private Button[] _squadNumberButtons;

    [Header("Slots Area")]
    // 5개의 슬롯 부모 (Grid Layout 등의 자식)
    [SerializeField] private RectTransform[] _slotParents;
    // 드래그 시 최상단에 그려질 레이어
    [SerializeField] private RectTransform _dragLayer;

    // 프리팹 내부에서 슬롯별로 미리 배치된 EmptyImage 참조 (Inspector 연결)
    [SerializeField] private GameObject[] _emptyImages;

    // 아이콘 배열
    [Header("Icons")]
    [SerializeField] private UI_NikkeIcon[] _nikkeIcons = new UI_NikkeIcon[5];

    [Header("Scroll Area")]
    [SerializeField] private UI_NikkeCardScrollView _cardScrollView; // 공통된 스크롤 뷰 사용

    [Header("Bottom Buttons")]
    [SerializeField] private Button _autoFormationButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton; // 닫기

    private SquadDetailPopupViewModel _viewModel;

    // 연출
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _autoFormationButton.onClick.AddListener(() => _viewModel?.OnClickAutoFormation());
        _resetButton.onClick.AddListener(() => _viewModel?.OnClickReset());
        _saveButton.onClick.AddListener(() => _viewModel?.OnClickSave());
        _cancelButton.onClick.AddListener(() => _viewModel?.OnClickClose());

        // 스쿼드 선택 버튼 리스너 바인딩
        if (_squadNumberButtons != null)
        {
            for (int i = 0; i < _squadNumberButtons.Length; i++)
            {
                int index = i; // 클로저 캡처
                _squadNumberButtons[i].onClick.AddListener(() => _viewModel?.SelectSquad(index));
            }
        }
    }

    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
            _viewModel.OnSquadDataChanged -= OnSquadDataChanged;
        }

        _viewModel = viewModel as SquadDetailPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        _viewModel.OnCloseRequested += OnCloseRequested;
        _viewModel.OnSquadDataChanged += OnSquadDataChanged;

        // Bind Properties
        Bind(_viewModel.TotalCombatPower, text => _combatPowerText.text = text);

        // 현재 선택된 인덱스에 따라 버튼 상태 갱신
        Bind(_viewModel.CurrentSquadIndex, UpdateSquadButtonStates);

        // Sub View Binding
        if (_cardScrollView != null)
            _cardScrollView.SetViewModel(_viewModel.ScrollViewModel);

        // Icons Init
        InitIcons();
    }

    /// <summary>
    /// 현재 선택된 스쿼드 인덱스에 맞춰 버튼의 Interactable 상태를 갱신합니다.
    /// 선택된 버튼은 비활성화하여 시각적으로 표시합니다.
    /// </summary>
    private void UpdateSquadButtonStates(int currentIndex)
    {
        if (_squadNumberButtons == null) return;

        for (int i = 0; i < _squadNumberButtons.Length; i++)
        {
            if (_squadNumberButtons[i] != null)
            {
                // 선택된 인덱스면 클릭 불가(선택됨 상태), 아니면 클릭 가능
                _squadNumberButtons[i].interactable = (i != currentIndex);
            }
        }
    }

    /// <summary>
    /// 인스펙터에 할당된 정적 아이콘 UI를 초기화하고 ViewModel을 연결합니다.
    /// </summary>
    private void InitIcons()
    {
        if (_nikkeIcons == null) return;

        for (int i = 0; i < 5; i++)
        {
            // 배열 범위를 벗어나거나 할당되지 않은 경우 스킵
            if (i >= _nikkeIcons.Length || _nikkeIcons[i] == null) continue;

            UI_NikkeIcon icon = _nikkeIcons[i];

            // 1. 초기화 (슬롯 인덱스, 드래그 레이어, 빈 이미지 참조)
            GameObject emptyImg = (i < _emptyImages.Length) ? _emptyImages[i] : null;
            icon.Initialize(i, _dragLayer, emptyImg);


            // 4. 뷰모델 연결
            if (_viewModel != null && _viewModel.SlotViewModels != null && i < _viewModel.SlotViewModels.Length)
            {
                icon.SetViewModel(_viewModel.SlotViewModels[i]);
            }

            // 프리팹에 미리 배치되므로 SetActive는 기본적으로 처리되어 있다고 가정하나, 확실히 하기 위해 켜줍니다.
            icon.gameObject.SetActive(true);
        }
    }

    private void OnSquadDataChanged()
    {
        ResetIconParents();
    }

    private void ResetIconParents()
    {
        if (_nikkeIcons == null) return;

        for (int i = 0; i < 5; i++)
        {
            if (i >= _nikkeIcons.Length || _nikkeIcons[i] == null) continue;
            if (i >= _slotParents.Length || _slotParents[i] == null) continue;

            UI_NikkeIcon icon = _nikkeIcons[i];
            Transform targetParent = _slotParents[i];

            // 드래그 중이 아니면 원래 부모로 복귀
            if (icon.transform.parent != targetParent)
            {
                icon.transform.SetParent(targetParent);
                icon.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                // 강제 하이라이트 끄기
                icon.SetHighlight(false);

                if (i < _emptyImages.Length && _emptyImages[i] != null)
                    _emptyImages[i].SetActive(false);
            }
        }
    }

    // --- Interaction Events Handlers ---

    // --- Interaction Events Handlers ---
    // Logic moved to ViewModel.

    // -----------------------------------


    // -----------------------------------

    private void OnEscapeAction(InputAction.CallbackContext ctx) => _viewModel?.OnClickClose();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        if (_fadeIn != null && _canvasGroup != null)
            await _fadeIn.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0)
    {
        if (_fadeOut != null && _canvasGroup != null)
            await _fadeOut.ExecuteAsync(_canvasGroup, delay);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        _autoFormationButton.onClick.RemoveAllListeners();
        _resetButton.onClick.RemoveAllListeners();
        _saveButton.onClick.RemoveAllListeners();
        _cancelButton.onClick.RemoveAllListeners();

        // x스쿼드 버튼 리스너 해제
        if (_squadNumberButtons != null)
        {
            foreach (var btn in _squadNumberButtons)
                if (btn != null) btn.onClick.RemoveAllListeners();
        }

        // 아이콘 이벤트 해제 (ViewModel이 처리하므로 불필요)


        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
            _viewModel.OnSquadDataChanged -= OnSquadDataChanged;
        }
    }
}