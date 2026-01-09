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
    [SerializeField] private TMP_Text _squadNameText;
    [SerializeField] private TMP_Text _combatPowerText;
    [SerializeField] private Button _prevSquadButton;
    [SerializeField] private Button _nextSquadButton;

    [Header("Slots Area")]
    // 5개의 슬롯 부모 (Grid Layout 등의 자식)
    [SerializeField] private RectTransform[] _slotParents;
    // 드래그 시 최상단에 그려질 레이어
    [SerializeField] private RectTransform _dragLayer;

    // 프리팹 내부에서 슬롯별로 미리 배치된 EmptyImage 참조 (Inspector 연결)
    [SerializeField] private GameObject[] _emptyImages;

    [Header("Scroll Area")]
    [SerializeField] private UI_NikkeCardScrollView _cardScrollView; // 공통된 스크롤 뷰 사용

    [Header("Bottom Buttons")]
    [SerializeField] private Button _autoFormationButton;
    [SerializeField] private Button _resetButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton; // 닫기

    private SquadDetailPopupViewModel _viewModel;
    private UI_NikkeIcon[] _spawnedIcons = new UI_NikkeIcon[5];

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

        _prevSquadButton.onClick.AddListener(OnPrevSquad);
        _nextSquadButton.onClick.AddListener(OnNextSquad);
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
        Bind(_viewModel.SquadName, text => _squadNameText.text = text);
        Bind(_viewModel.TotalCombatPower, text => _combatPowerText.text = text);

        // Sub View Binding
        if (_cardScrollView != null)
            _cardScrollView.SetViewModel(_viewModel.ScrollViewModel);

        // Icons Init
        InitIcons();
    }

    private void OnPrevSquad()
    {
        if (_viewModel == null) return;
        int current = _viewModel.CurrentSquadIndex.Value;
        if (current > 0) _viewModel.SelectSquad(current - 1);
    }

    private void OnNextSquad()
    {
        if (_viewModel == null) return;
        int current = _viewModel.CurrentSquadIndex.Value;
        if (current < 4) _viewModel.SelectSquad(current + 1);
    }

    /// <summary>
    /// 초기 실행 시 슬롯에 아이콘 UI를 생성하고 ViewModel을 연결합니다.
    /// </summary>
    private async void InitIcons()
    {
        for (int i = 0; i < 5; i++)
        {
            if (i >= _slotParents.Length) break;

            // 이미 생성된 경우 재사용하지 않고 새로 생성 (심플함을 위해)
            // 실제로는 풀링하거나 재사용하는 것이 좋음.
            if (_spawnedIcons[i] != null)
            {
                Managers.UI.Close(_spawnedIcons[i]);
                _spawnedIcons[i] = null;
            }

            // 아이콘 생성
            UI_NikkeIcon icon = await Managers.UI.ShowAsync<UI_NikkeIcon>(null, _slotParents[i]);
            if (icon != null)
            {
                _spawnedIcons[i] = icon;

                // 초기화 (슬롯 인덱스, 드래그 레이어, 스왑 콜백, 빈 이미지 참조)
                GameObject emptyImg = (i < _emptyImages.Length) ? _emptyImages[i] : null;
                icon.Initialize(i, _dragLayer, OnSwapRequest, emptyImg);

                // 뷰모델 연결
                icon.SetViewModel(_viewModel.SlotViewModels[i]);
            }
        }
    }

    private void OnSquadDataChanged()
    {
        ResetIconParents();
    }

    private void ResetIconParents()
    {
        for (int i = 0; i < 5; i++)
        {
            if (_spawnedIcons[i] != null && _slotParents[i] != null)
            {
                // 드래그 중이 아니면 원래 부모로 복귀
                if (_spawnedIcons[i].transform.parent != _slotParents[i])
                {
                    _spawnedIcons[i].transform.SetParent(_slotParents[i]);
                    _spawnedIcons[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    if (i < _emptyImages.Length && _emptyImages[i] != null)
                        _emptyImages[i].SetActive(false);
                }
            }
        }
    }

    private void OnSwapRequest(int fromIndex, int toIndex)
    {
        _viewModel?.SwapSlot(fromIndex, toIndex);
    }

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
        _prevSquadButton.onClick.RemoveAllListeners();
        _nextSquadButton.onClick.RemoveAllListeners();

        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
            _viewModel.OnSquadDataChanged -= OnSquadDataChanged;
        }

        // 아이콘 정리
        foreach (var icon in _spawnedIcons)
        {
            if (icon != null) Managers.UI.Close(icon);
        }
    }
}