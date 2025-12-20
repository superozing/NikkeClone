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

    [SerializeField] private UI_NikkeCardSortFilter _sortFilterView; // 프리팹 바인딩
    [SerializeField] private Button _sortButton; // 필터 뷰 열기 버튼

    [SerializeField] private Button _sortOrderButton; // 오름차순/내림차순 토글 버튼
    [SerializeField] private TMP_Text _sortTypeText; // 현재 정렬 기준 텍스트 ("전투력", "레벨")
    [SerializeField] private Image _sortArrowImage; // 정렬 방향 화살표

    [Header("Buttons")]
    [SerializeField] private Button _searchButton;
    [SerializeField] private Button _burst1Button;
    [SerializeField] private Button _burst2Button;
    [SerializeField] private Button _burst3Button;

    private NikkeCardScrollViewModel _viewModel;
    private readonly List<UI_NikkeCard> _cardInstances = new();

    // 버튼 등장시키기 위한 연출 클래스
    private readonly IUIAnimation _buttonGroupFadeIn = new FadeInUIAnimation(0.2f, Ease.OutQuad);

    // 색상 정의
    private readonly Color _activeColor = new Color(.2f, .7f, .9f);
    private readonly Color _inactiveColor = new Color(.2f, .2f, .2f);

    protected override void Awake()
    {
        base.Awake();
        
        _searchButton.onClick.AddListener(_viewModel.OnClickSearch);
        _sortButton.onClick.AddListener(_viewModel.RequestOpenSortFilter);
        _sortOrderButton.onClick.AddListener(_viewModel.ToggleSortOrder);

        _burst1Button.onClick.AddListener(() => _viewModel?.OnClickBurst(1));
        _burst2Button.onClick.AddListener(() => _viewModel?.OnClickBurst(2));
        _burst3Button.onClick.AddListener(() => _viewModel?.OnClickBurst(3));

        // 초기에는 필터 뷰 비활성화
        _sortFilterView.gameObject.SetActive(false);
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnListUpdated -= RefreshScroll;
            _viewModel.OnNikkeClickCallback -= OnNikkeClicked;
            _viewModel.OnControlSortFilterView -= OnControlSortFilterView;
        }

        _viewModel = viewModel as NikkeCardScrollViewModel;
        base.SetViewModel(viewModel);

        // 자식 뷰인 SortFilterView에도 ViewModel 주입
        _sortFilterView.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnListUpdated += RefreshScroll;
            _viewModel.OnNikkeClickCallback += OnNikkeClicked;
            _viewModel.OnControlSortFilterView += OnControlSortFilterView;
            
            Bind(_viewModel.IsSearchActive,     isActive => SetButtonColor(_searchButton, isActive));
            Bind(_viewModel.SortType,           OnSortTypeChanged);
            Bind(_viewModel.IsSortAscending,    OnSortOrderChanged);

            Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst1], isActive => SetButtonColor(_burst1Button, isActive));
            Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst2], isActive => SetButtonColor(_burst2Button, isActive));
            Bind(_viewModel.BurstFilters[(int)eNikkeBurst.Burst3], isActive => SetButtonColor(_burst3Button, isActive));

            // 비동기 초기화 및 갱신 시작
            InitAndRefreshAsync();
        }
    }

    private async void OnControlSortFilterView(bool isOpen)
    {
        // 1. 버튼 색상 갱신 (상태에 맞게)
        SetButtonColor(_sortButton, isOpen);

        // 2. 뷰 활성/비활성 처리
        if (isOpen)
            _sortFilterView.gameObject.SetActive(true);
        else
            _ = _sortFilterView.CloseAsync();
    }

    private void OnSortTypeChanged(eNikkeSortType type)
    {
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
        float scaleY = isAscending ? 1f : -1f;
        _sortArrowImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
    }

    private void SetButtonColor(Button button, bool isActive)
    {
        if (button != null && button.TryGetComponent<Image>(out var img))
            img.color = isActive ? _activeColor : _inactiveColor;
    }

    private void OnNikkeClicked(int nikkeId)
    {
        Debug.Log($"[UI_NikkeCardScrollView] 니케 클릭됨: ID {nikkeId}");
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
        if (_viewModel.DisplayNikkes == null) return;

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
    public void PlayButtonActiveAnimation()
    {
        float defaultDelay = 0.3f; // 기본 대기시간
        _buttonGroupFadeIn.ExecuteAsync(_buttonGroup, defaultDelay);
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
        }
    }
}