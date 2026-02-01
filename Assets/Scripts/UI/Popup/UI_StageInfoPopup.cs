using System.Threading.Tasks;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 스테이지 정보 팝업 View입니다.
/// 캠페인 씬에서 스쿼드가 스테이지와 충돌했을 때 표시됩니다.
/// </summary>
public class UI_StageInfoPopup : UI_Popup, IUIShowHideAnimation
{
    public override string ActionMapKey => "UI_StageInfoPopup";

    // --- Header ---
    [Header("Header")]
    [SerializeField] private TMP_Text _txtStageName;
    [SerializeField] private TMP_Text _txtStageType;

    // --- 스쿼드 정보 ---
    [Header("Squad Info")]
    [SerializeField] private Button[] _squadNumberButtons; // 5개 스쿼드 선택 버튼
    [SerializeField] private UI_NikkeIcon[] _nikkeIcons;  // 5개 고정
    [SerializeField] private TMP_Text _txtCurrentCombatPower;  // 현재 스쿼드 전투력
    [SerializeField] private TMP_Text _txtReferenceCombatPower; // 스테이지 기준 전투력

    // --- 버튼 ---
    [Header("Buttons")]
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnCombat;
    [SerializeField] private Button _btnStageType;         // TODO: 스테이지 타입 팝업
    [SerializeField] private Button _btnStageWaveInfo;     // TODO: 스테이지 웨이브 정보 팝업

    // --- Sub UI ---
    [Header("Sub UI")]
    [SerializeField] private UI_StageWeakCodeInfo _weakCodeInfo;
    [SerializeField] private UI_StageRangeInfo _rangeInfo;
    [SerializeField] private UI_StageRewardInfo _rewardInfo;

    private StageInfoPopupViewModel _viewModel;

    // --- 연출 ---
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 스쿼드 선택 버튼 리스너
        if (_squadNumberButtons != null)
        {
            for (int i = 0; i < _squadNumberButtons.Length; i++)
            {
                int index = i;
                _squadNumberButtons[i]?.onClick.AddListener(() => _viewModel?.SelectSquad(index));
            }
        }

        _btnCombat?.onClick.AddListener(() => _viewModel?.RequestBattle());
        _btnClose?.onClick.AddListener(() => _viewModel?.RequestClose());

        _btnStageType?.onClick.AddListener(() => { /* TODO: 스테이지 타입 팝업 */ });
        _btnStageWaveInfo?.onClick.AddListener(() => { /* TODO: 웨이브 정보 팝업 */ });
    }

    protected async void OnEnable()
    {
        await PlayShowAnimationAsync();
    }

    /// <summary>
    /// ViewModel을 설정하고 데이터 바인딩을 수행합니다.
    /// </summary>
    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 기존 이벤트 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        _viewModel = viewModel as StageInfoPopupViewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 2. 이벤트 구독
        _viewModel.OnCloseRequested += OnCloseRequested;

        // 3. ReactiveProperty 바인딩
        Bind(_viewModel.StageName, text => _txtStageName.text = text);
        Bind(_viewModel.StageTypeName, text => _txtStageType.text = text);
        Bind(_viewModel.ReferenceCombatPower, power => _txtReferenceCombatPower.text = power.ToString("N0"));

        // 추가 바인딩
        Bind(_viewModel.CurrentCombatPower, power => _txtCurrentCombatPower.text = power.ToString("N0"));

        // 스쿼드 선택 시 UI 갱신 (Bind 시 즉시 호출되어 초기 아이콘 설정됨)
        Bind(_viewModel.CurrentSquadIndex, OnSquadChanged);

        // 4. Sub-UI ViewModel 연결
        _weakCodeInfo?.SetViewModel(_viewModel.WeakCodeInfo);
        _rangeInfo?.SetViewModel(_viewModel.RangeInfo);
        _rewardInfo?.SetViewModel(_viewModel.RewardInfo);
    }

    private void OnSquadChanged(int newSquadIndex)
    {
        UpdateSquadButtonStates(newSquadIndex);
        BindCurrentSquadIcons();
    }

    /// <summary>
    /// 현재 선택된 스쿼드의 NikkeIcon ViewModels를 UI_NikkeIcon에 바인딩합니다.
    /// </summary>
    private void BindCurrentSquadIcons()
    {
        var currentIcons = _viewModel.CurrentSquadNikkeIcons;

        for (int i = 0; i < 5; ++i)
        {
            // DisplayMode로 초기화 (drag 비활성화는 ViewModel이 담당)
            _nikkeIcons[i].Initialize(
                slotIndex: i,
                dragLayer: null,
                emptyImage: null
            );

            _nikkeIcons[i].SetViewModel(currentIcons[i]);
            _nikkeIcons[i].gameObject.SetActive(true);
        }
    }

    private void UpdateSquadButtonStates(int currentIndex)
    {
        if (_squadNumberButtons == null) return;
        for (int i = 0; i < _squadNumberButtons.Length; i++)
        {
            if (_squadNumberButtons[i] != null)
                _squadNumberButtons[i].interactable = (i != currentIndex);
        }
    }

    private void OnEscapeAction(InputAction.CallbackContext ctx) => _viewModel?.RequestClose();

    private async void OnCloseRequested()
    {
        await PlayHideAnimationAsync();
        Managers.UI.Close(this);
    }

    // --- IUIShowHideAnimation 구현 ---

    public async Task PlayShowAnimationAsync(float delay = 0)
    {
        await _fadeIn.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0)
    {
        await _fadeOut.ExecuteAsync(_canvasGroup, delay);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Managers.Input.UnbindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 버튼 리스너 해제
        if (_squadNumberButtons != null)
        {
            foreach (var btn in _squadNumberButtons)
                btn?.onClick.RemoveAllListeners();
        }
        _btnCombat?.onClick.RemoveAllListeners();
        _btnClose?.onClick.RemoveAllListeners();
        _btnStageType?.onClick.RemoveAllListeners();
        _btnStageWaveInfo?.onClick.RemoveAllListeners();

        // 이벤트 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }
    }
}
