using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_NikkeCardScrollView : UI_View
{
    [Header("Content Area")]
    [SerializeField] private RectTransform _content;
    [SerializeField] private CanvasGroup _buttonGroup; // 연출을 위해

    [Header("Sort/Filter UI")]
    [SerializeField] private Button _sortButton;
    private UI_NikkeCardSortFilter _sortFilterView;

    [SerializeField] private Button _sortOrderButton; // 오름차순/내림차순 토글 버튼
    [SerializeField] private TMP_Text _sortTypeText; // 현재 정렬 기준 텍스트 ("전투력", "레벨")
    [SerializeField] private Image _sortArrowImage; // 정렬 방향 화살표

    [Header("Buttons")]
    [SerializeField] private Button _searchButton;
    [SerializeField] private Button _burst1Button;
    [SerializeField] private Button _burst2Button;
    [SerializeField] private Button _burst3Button;

    // 인터페이스로 변경하여 다형성 지원
    private INikkeCardScrollViewModel _viewModel;
    private readonly List<UI_NikkeCard> _cardInstances = new();

    // 버튼 등장시키기 위한 연출 클래스
    private IUIAnimation _buttonGroupFade;

    // 색상 정의
    private readonly Color _activeColor = new Color(.2f, .7f, .9f);
    private readonly Color _inactiveColor = new Color(.2f, .2f, .2f);

    protected override void Awake()
    {
        base.Awake();
        _buttonGroupFade = new FadeUIAnimation(_buttonGroup, 0f, 1f, 0.2f, Ease.OutQuad);

        // null 체크를 추가하여 버튼이 없는 View에서도 오류가 나지 않도록 함 (SquadDetail 등에서 재사용 시)
        if (_searchButton) _searchButton.onClick.AddListener(() => _viewModel?.OnClickSearch());
        if (_sortButton) _sortButton.onClick.AddListener(() => _viewModel?.RequestOpenSortFilter());
        if (_sortOrderButton) _sortOrderButton.onClick.AddListener(() => _viewModel?.ToggleSortOrder());

        if (_burst1Button) _burst1Button.onClick.AddListener(() => _viewModel?.OnClickBurst(1));
        if (_burst2Button) _burst2Button.onClick.AddListener(() => _viewModel?.OnClickBurst(2));
        if (_burst3Button) _burst3Button.onClick.AddListener(() => _viewModel?.OnClickBurst(3));
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnListUpdated -= RefreshScroll;
            _viewModel.OnNikkeClickCallback -= OnNikkeClicked;
            _viewModel.OnControlSortFilterView -= OnControlSortFilterView;
        }

        // 인터페이스로 캐스팅
        _viewModel = viewModel as INikkeCardScrollViewModel;

        // Base 호출 (ViewModelBase로 업캐스팅해서 넘겨줌)
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnListUpdated += RefreshScroll;
            _viewModel.OnNikkeClickCallback += OnNikkeClicked;
            _viewModel.OnControlSortFilterView += OnControlSortFilterView;

            if (_searchButton) Bind(_viewModel.IsSearchActive, isActive => SetButtonColor(_searchButton, isActive));
            Bind(_viewModel.SortType, OnSortTypeChanged);
            Bind(_viewModel.IsSortAscending, OnSortOrderChanged);

            if (_burst1Button) Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst1], isActive => SetButtonColor(_burst1Button, isActive));
            if (_burst2Button) Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst2], isActive => SetButtonColor(_burst2Button, isActive));
            if (_burst3Button) Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst3], isActive => SetButtonColor(_burst3Button, isActive));

            // 비동기 초기화 및 갱신 시작
            InitAndRefreshAsync();
        }
    }

    bool _filterViewOpenState = false;
    private async void OnControlSortFilterView(bool isOpen)
    {
        if (_filterViewOpenState == isOpen)
            return;

        _filterViewOpenState = isOpen;

        // 1. 버튼 색상 갱신 (상태에 맞게)
        SetButtonColor(_sortButton, isOpen);

        // 2. 뷰 활성/비활성 처리
        if (isOpen)
            _sortFilterView = await Managers.UI.ShowAsync<UI_NikkeCardSortFilter>(ViewModel);
        else if (_sortFilterView != null)
            _ = _sortFilterView.CloseAsync();
    }

    private void OnSortTypeChanged(eNikkeSortType type)
    {
        if (_sortTypeText == null) return;

        string text = type switch
        {
            eNikkeSortType.CombatPower => "전투력",
            eNikkeSortType.Level => "레벨",
            _ => type.ToString()
        };

        _sortTypeText.text = text;
    }

    private void OnSortOrderChanged(bool isAscending)
    {
        if (_sortArrowImage == null) return;
        float scaleY = isAscending ? 1f : -1f;
        _sortArrowImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
    }

    private void SetButtonColor(Button button, bool isActive)
    {
        if (button != null && button.TryGetComponent<Image>(out var img))
            img.color = isActive ? _activeColor : _inactiveColor;
    }

    // 뷰모델에서 전달받은 ID를 그대로 처리 (상세팝업 or 선택로직은 뷰모델이 결정)
    private void OnNikkeClicked(int nikkeId)
    {
        // 뷰는 단순히 뷰모델의 명령을 수행하는 구조였으나, 
        // 인터페이스 설계상 OnNikkeClickCallback은 뷰모델 -> 뷰 방향의 이벤트임.
        // 기존: View Click -> ViewModel Method -> Action Invoke -> View Response
        // 변경: 
        // 1. View Card Click -> ViewModel.OnClick -> (ViewModel 내부 로직 수행)
        // 2. 만약 View에서 추가로 처리할 UI적 연출이 있다면 여기서 처리.
        // 현재는 ViewModel 내부에서 Popup을 띄우거나 데이터를 수정하므로 View가 할 일은 딱히 없음.
    }

    /// <summary>
    /// 비동기 초기화 프로세스를 수행합니다.
    /// </summary>
    private async void InitAndRefreshAsync()
    {
        await InitialSpawn();
        RefreshScroll();
    }

    /// <summary>
    /// 뷰모델에 존재하는 모든 니케 수만큼 UIManager를 통해 UI 객체를 미리 생성해둡니다.
    /// </summary>
    private async Task InitialSpawn()
    {
        if (_content == null) return;

        int currentCount = _cardInstances.Count;
        int targetCount = _viewModel.TotalNikkeCount;

        for (int i = currentCount; i < targetCount; i++)
        {
            UI_NikkeCard uiCard = await Managers.UI.ShowAsync<UI_NikkeCard>(null, _content);

            if (uiCard != null)
            {
                // 생성 직후엔 비활성화 (Pool 역할)
                uiCard.gameObject.SetActive(false);
                _cardInstances.Add(uiCard);
            }
        }
    }

    private async void RefreshScroll()
    {
        if (_viewModel?.DisplayNikkes == null) return;

        var displayList = _viewModel.DisplayNikkes;
        int displayCount = displayList.Count;

        // 1. 표시할 데이터 수만큼 활성화 및 ViewModel 주입
        for (int i = 0; i < displayCount; i++)
        {
            // 만약 InitialSpawn 이후 데이터가 늘어났다면 안전장치로 추가 생성
            if (i >= _cardInstances.Count)
            {
                UI_NikkeCard uiCard = await Managers.UI.ShowAsync<UI_NikkeCard>(null, _content);
                if (uiCard != null) _cardInstances.Add(uiCard);
            }

            // 비동기 생성 중 오류 등으로 리스트에 null이 들어갈 가능성 방어
            if (i < _cardInstances.Count && _cardInstances[i] != null)
            {
                var cardUI = _cardInstances[i];
                cardUI.gameObject.SetActive(true);
                cardUI.SetViewModel(displayList[i]);
            }
        }

        // 2. 나머지 비활성화 및 ViewModel 해제
        for (int i = displayCount; i < _cardInstances.Count; i++)
        {
            var cardUI = _cardInstances[i];
            if (cardUI != null)
            {
                cardUI.SetViewModel(null); // 바인딩 해제
                cardUI.gameObject.SetActive(false);
            }
        }

        // 3. 순차 연출 실행
        PlayCardRefreshAnimation();
    }

    /// <summary>
    /// 탭 진입 시 호출되는 메서드입니다.
    /// 버튼 그룹 등장 연출과 카드 리스트 등장 연출을 모두 실행합니다.
    /// </summary>
    public void PlayActiveAnimation()
    {
        PlayButtonActiveAnimation();
        PlayCardRefreshAnimation();
    }

    /// <summary>
    /// 버튼 그룹에 등장 연출을 재생합니다.
    /// </summary>
    public async void PlayButtonActiveAnimation()
    {
        if (_buttonGroupFade != null)
        {
            await _buttonGroupFade.ExecuteAsync(0.3f);

            if (_buttonGroup != null)
                _buttonGroup.interactable = true;
        }
    }

    /// <summary>
    /// 활성화된 카드들에 대해 순차적으로 등장 연출을 재생합니다.
    /// </summary>
    public void PlayCardRefreshAnimation()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);

        float interval = 0.02f; // 카드 간 간격
        float defaultDelay = 0.5f; // 기본 대기시간

        for (int i = 0; i < _cardInstances.Count; ++i)
        {
            var card = _cardInstances[i];

            // 활성화된 카드만 연출
            if (!card.gameObject.activeSelf) continue;

            // 딜레이 계산
            float delay = defaultDelay + (i * interval);
            _ = card.PlayShowAnimationAsync(delay);
        }
    }

    protected void OnDisable()
    {
        // 뷰모델의 상태를 초기화하고 팝업을 닫도록 요청
        _viewModel?.ResetFiltersAndPopup();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (var card in _cardInstances)
        {
            // UI_NikkeCard 해제
            if (card != null)
                Managers.UI.Close(card);
        }

        _cardInstances.Clear();

        if (_viewModel != null)
        {
            _viewModel.OnListUpdated -= RefreshScroll;
            _viewModel.OnNikkeClickCallback -= OnNikkeClicked;
            _viewModel.OnControlSortFilterView -= OnControlSortFilterView;
        }
    }
}