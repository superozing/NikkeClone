using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_NikkeCardScrollView : UI_View
{
    [Header("Content Area")]
    [SerializeField] private RectTransform _content;

    [Header("Buttons")]
    [SerializeField] private Button _searchButton;
    [SerializeField] private Button _sortButton;
    [SerializeField] private Button _burst1Button;
    [SerializeField] private Button _burst2Button;
    [SerializeField] private Button _burst3Button;

    private NikkeCardScrollViewModel _viewModel;
    private readonly List<UI_NikkeCard> _cardInstances = new();

    // 색상 정의
    private readonly Color _activeColor = new Color(0.2f, 0.2f, 1f);
    private readonly Color _inactiveColor = new Color(.2f, .2f, .2f);

    // 연출 취소 용도
    private CancellationTokenSource _seqCts;

    protected override void Awake()
    {
        base.Awake();
        BindButtonEvents();
    }

    private void BindButtonEvents()
    {
        _searchButton.onClick.AddListener(() => _viewModel?.OnClickSearch());
        _sortButton.onClick.AddListener(() => _viewModel?.OnClickSort());

        _burst1Button.onClick.AddListener(() => _viewModel?.OnClickBurst(1));
        _burst2Button.onClick.AddListener(() => _viewModel?.OnClickBurst(2));
        _burst3Button.onClick.AddListener(() => _viewModel?.OnClickBurst(3));
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
        {
            _viewModel.OnListUpdated -= RefreshScroll;
            _viewModel.OnNikkeClickCallback -= OnNikkeClicked;
        }

        _viewModel = viewModel as NikkeCardScrollViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnListUpdated += RefreshScroll;
            _viewModel.OnNikkeClickCallback += OnNikkeClicked;

            // 버튼 색상 상태 바인딩
            Bind(_viewModel.IsSearchActive, isActive => UpdateButtonColor(_searchButton, isActive));
            Bind(_viewModel.IsSortActive, isActive => UpdateButtonColor(_sortButton, isActive));
            Bind(_viewModel.IsBurst1Active, isActive => UpdateButtonColor(_burst1Button, isActive));
            Bind(_viewModel.IsBurst2Active, isActive => UpdateButtonColor(_burst2Button, isActive));
            Bind(_viewModel.IsBurst3Active, isActive => UpdateButtonColor(_burst3Button, isActive));

            // 비동기 초기화 및 갱신 시작
            InitAndRefreshAsync();
        }
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
            // Managers.UI.ShowAsync를 사용하여 생성 (기존 로직 활용)
            // parent를 _content로 지정하여 스크롤 뷰 안에 생성되도록 함
            // ViewModel은 RefreshScroll에서 설정하므로 여기서는 null 전달
            UI_NikkeCard uiCard = await Managers.UI.ShowAsync<UI_NikkeCard>(null, _content);

            if (uiCard != null)
            {
                // 생성 직후엔 비활성화 (Pool 역할)
                uiCard.gameObject.SetActive(false);
                _cardInstances.Add(uiCard);
            }
        }
    }

    private void UpdateButtonColor(Button button, bool isActive)
    {
        if (button == null)
            return;

        if (button.TryGetComponent<Image>(out var targetImage))
        {
            targetImage.color = isActive ? _activeColor : _inactiveColor;
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
        PlayAnimationSequence();
    }

    /// <summary>
    /// 활성화된 카드들에 대해 순차적으로 등장 연출을 재생합니다.
    /// </summary>
    public async void PlayAnimationSequence()
    {
        // 기존 연출 취소
        _seqCts?.Cancel();
        _seqCts = new CancellationTokenSource();
        var token = _seqCts.Token;

        try
        {
            foreach (var card in _cardInstances)
            {
                if (token.IsCancellationRequested) return;

                // 활성화된 카드만 연출
                if (!card.gameObject.activeSelf) continue;

                _ = card.PlayShowAnimationAsync();

                // 다음 카드 연출 전 딜레이
                // 굳이 이렇게 기다리는 이유는 연출 클래스를 새로 감싸서 생성하거나 기존 연출 클래스를 수정하는 것보다 좋아보여서.
                await Task.Delay(50, token);
            }
        }
        catch (TaskCanceledException)
        {
            // 연출 중단
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _seqCts?.Cancel();
        _seqCts?.Dispose();

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