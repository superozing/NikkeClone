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
    [SerializeField] private Button _btnSquad;
    [SerializeField] private UI_NikkeIcon[] _nikkeIcons;  // 5개 고정
    [SerializeField] private TMP_Text _txtReferenceCombatPower;

    // --- 버튼 ---
    [Header("Buttons")]
    [SerializeField] private Button _btnClose;
    [SerializeField] private Button _btnBattle;
    [SerializeField] private Button _btnBattleMethod;      // TODO: 전투방식 팝업
    [SerializeField] private Button _btnBattleFieldInfo;   // TODO: 전장정보 팝업

    // --- Sub UI ---
    [Header("Sub UI")]
    [SerializeField] private UI_StageWeakCodeInfo _weakCodeInfo;
    [SerializeField] private UI_StageRangeInfo _rangeInfo;
    [SerializeField] private UI_StageRewardInfo _rewardInfo;
    [SerializeField] private Transform _battleFieldInfoSlot;
    [SerializeField] private Transform _enemyInfoSlot;

    private StageInfoPopupViewModel _viewModel;

    // --- 연출 ---
    private readonly IUIAnimation _fadeIn = new FadeInUIAnimation(0.2f);
    private readonly IUIAnimation _fadeOut = new FadeOutUIAnimation(0.2f);

    protected override void Awake()
    {
        base.Awake();
        Managers.Input.BindAction("Close", OnEscapeAction, InputActionPhase.Performed);

        // 버튼 리스너 등록
        _btnSquad?.onClick.AddListener(OnSquadButtonClicked);
        _btnBattle?.onClick.AddListener(OnBattleClicked);
        _btnClose?.onClick.AddListener(OnCloseClicked);

        // TODO 버튼들 (미구현)
        _btnBattleMethod?.onClick.AddListener(() => { /* TODO: 전투방식 팝업 */ });
        _btnBattleFieldInfo?.onClick.AddListener(() => { /* TODO: 전장정보 팝업 */ });
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

        // 4. NikkeIcon 초기화 및 ViewModel 연결
        for (int i = 0; i < 5; ++i)
        {
            _nikkeIcons[i].SetViewModel(_viewModel.NikkeIcons[i]);
            _nikkeIcons[i].gameObject.SetActive(true);
        }

        // 5. Sub-UI ViewModel 연결
        _weakCodeInfo?.SetViewModel(_viewModel.WeakCodeInfo);
        _rangeInfo?.SetViewModel(_viewModel.RangeInfo);
        _rewardInfo?.SetViewModel(_viewModel.RewardInfo);
    }

    // --- 버튼 클릭 핸들러 ---

    /// <summary>
    /// 스쿼드 버튼 클릭 시 UI_SquadDetailPopup을 엽니다.
    /// </summary>
    private void OnSquadButtonClicked()
    {
        _viewModel?.RequestSquadEdit();
    }

    /// <summary>
    /// 전투 버튼 클릭 시 전투 시작을 요청합니다.
    /// </summary>
    private void OnBattleClicked()
    {
        _viewModel?.RequestBattle();
    }

    /// <summary>
    /// 닫기 버튼 클릭 시 팝업을 닫습니다.
    /// </summary>
    private void OnCloseClicked()
    {
        _viewModel?.RequestClose();
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
        _btnSquad?.onClick.RemoveAllListeners();
        _btnBattle?.onClick.RemoveAllListeners();
        _btnClose?.onClick.RemoveAllListeners();
        _btnBattleMethod?.onClick.RemoveAllListeners();
        _btnBattleFieldInfo?.onClick.RemoveAllListeners();

        // 이벤트 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }
    }
}
