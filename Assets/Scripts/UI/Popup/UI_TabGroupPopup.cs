using System;
using UI;
using UnityEngine;

public class UI_TabGroupPopup : UI_Popup
{
    public override string ActionMapKey => "UI_TabGroupPopup";

    [SerializeField] private Transform _tabContentRoot;
    [SerializeField] private UI_TabButtonGroup _tabButtonGroup;

    private TabGroupPopupViewModel _viewModel;

    private UI_TabBase[] _tabs = new UI_TabBase[(int)eTabType.End];
    private UI_TabBase _curTab;

    protected override async void Awake()
    {
        base.Awake();

        if (_tabButtonGroup == null)
            Debug.LogError("[UI_TabGroupPopup] _tabButtonsGroup가 Inspector에서 할당되지 않았습니다.");

        // 1. 탭 생성
        _tabs[(int)eTabType.Lobby] = await Managers.UI.ShowAsync<UI_LobbyTab>(new LobbyTabViewModel(), _tabContentRoot);
        _tabs[(int)eTabType.Squad] = await Managers.UI.ShowAsync<UI_SquadTab>(new SquadTabViewModel(), _tabContentRoot);
        _tabs[(int)eTabType.Nikke] = await Managers.UI.ShowAsync<UI_NikkeTab>(new NikkeTabViewModel(), _tabContentRoot);
        _tabs[(int)eTabType.Inventory] = await Managers.UI.ShowAsync<UI_InventoryTab>(new InventoryTabViewModel(), _tabContentRoot);
        _tabs[(int)eTabType.Recruit] = await Managers.UI.ShowAsync<UI_RecruitTab>(new RecruitTabViewModel(), _tabContentRoot);

        // 2. 모든 탭 비활성화
        for (int i = 0; i < _tabs.Length; ++i)
            _tabs[i].OnTabDeselected();

        // 3. (뷰모델 세팅 시)초기 탭 활성화
        if (_viewModel != null)
            ChangeTab(_viewModel.CurrentTabType);
    }

    public override void SetViewModel(IViewModel viewModel)
    {
        _viewModel = viewModel as TabGroupPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_TabGroupPopup] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        base.SetViewModel(_viewModel);

        // 자식에도 뷰모델 세팅
        if (_tabButtonGroup != null)
            _tabButtonGroup?.SetViewModel(_viewModel);
    }

    protected override void OnStateChanged()
    {
        if (_viewModel == null) 
            return;

        ChangeTab(_viewModel.CurrentTabType);
    }

    /// <summary>
    /// 지정된 타입의 탭으로 전환합니다.
    /// </summary>
    /// <param name="type">활성화 할 탭 타입</param>
    private void ChangeTab(eTabType type)
    {
        UI_TabBase nextTab = _tabs[(int)type];

        // 탭이 아직 생성되지 않았거나 현재 탭과 같을 경우 예외처리
        if (nextTab == null || _curTab == nextTab)
            return;

        // 1. 현재 탭 비활성화
        if (_curTab != null)
            _curTab.OnTabDeselected();

        // 2. 새 탭 활성화
        nextTab.OnTabSelected();
        _curTab = nextTab;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_tabs != null)
        {
            for (int i = 0; i < _tabs.Length; ++i)
                if (_tabs[i] != null)
                    Managers.UI.Close(_tabs[i]);
        }

        _tabs = null;

        (_viewModel as IDisposable)?.Dispose();
        _viewModel = null;
    }
}