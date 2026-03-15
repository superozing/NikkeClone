using System;
using UI;
using UnityEngine;

public class UI_TabGroupPopup : UI_Popup
{
    public override string ActionMapKey => "UI_TabGroupPopup";

    [SerializeField] private UI_TabBase[] _tabs = new UI_TabBase[(int)eTabType.End];
    [SerializeField] private UI_TabButtonGroup _tabButtonGroup;
    [SerializeField] private UI_Money _moneyView;

    private TabGroupPopupViewModel _viewModel;
    private UI_TabBase _curTab;

    protected override void Awake()
    {
        base.Awake();

        // 기본적으로 모든 탭 비활성화
        for (int i = 0; i < _tabs.Length; ++i)
        {
            if (_tabs[i] == null)
                Debug.LogError($"[UI_TabGroupPopup] _tabs 배열의 인덱스 {i} ({(eTabType)i})가 Inspector에서 할당되지 않았습니다.");

            _tabs[i].OnTabDeselected();
        }
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as TabGroupPopupViewModel;
        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_TabGroupPopup] 잘못된 ViewModel 타입이 주입되었습니다.");
            return;
        }

        base.SetViewModel(_viewModel);

        if (_viewModel == null)
            return;

        // 1. 탭 버튼 그룹 ViewModel 설정
        if (_tabButtonGroup != null)
            _tabButtonGroup.SetViewModel(_viewModel);

        // 2. 개별 탭 ViewModel 설정
        for (int i = 0; i < _tabs.Length; ++i)
            _tabs[i].SetViewModel(_viewModel.TabViewModels[i]);

        // 3. 재화 UI ViewModel 설정 (추가됨)
        if (_moneyView != null)
            _moneyView.SetViewModel(_viewModel.MoneyViewModel);

        Bind(_viewModel.CurrentTabType, ChangeTab);
    }

    /// <summary>
    /// 지정된 타입의 탭으로 전환합니다.
    /// </summary>
    /// <param name="type">활성화 할 탭 타입</param>
    private void ChangeTab(eTabType type)
    {
        // 같은 탭일 경우 예외처리
        if (_curTab == _tabs[(int)type])
            return;

        // 1. 현재 탭 비활성화
        if (_curTab != null)
            _curTab.OnTabDeselected();

        // 2. 새 탭 활성화
        _curTab = _tabs[(int)type];
        _curTab.OnTabSelected();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _tabs = null;
        _viewModel = null;
    }
}