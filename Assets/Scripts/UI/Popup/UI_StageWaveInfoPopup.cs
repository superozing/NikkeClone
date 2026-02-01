using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// StageWaveInfoPopup View입니다.
/// 스테이지 정보 및 랩쳐 정보를 표시합니다.
/// </summary>
public class UI_StageWaveInfoPopup : UI_Popup
{
    [Header("Header")]
    [SerializeField] private TMP_Text _txtStageType;
    [SerializeField] private TMP_Text _txtStageName;

    [Header("Sub-UI")]
    [SerializeField] private UI_StageWeakCodeInfo _weakCodeInfo;
    [SerializeField] private UI_StageRangeInfo _rangeInfo;

    [Header("Rapture Section")]
    [SerializeField] private Transform _targetIconContainer;      // Grade 3
    [SerializeField] private Transform _subTargetIconContainer;   // Grade 2
    [SerializeField] private Transform _soldierIconContainer;     // Grade 1

    [Header("Buttons")]
    [SerializeField] private Button _btnClose;

    private StageWaveInfoPopupViewModel _viewModel;
    private List<UI_RaptureIcon> _spawnedIcons = new();

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 1. 기존 ViewModel 이벤트 해제
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        // 2. 부모 SetViewModel 호출 (기존 바인딩 해제 및 교체)
        base.SetViewModel(viewModel);

        _viewModel = viewModel as StageWaveInfoPopupViewModel;
        if (_viewModel == null) return;

        // 3. 이벤트 구독
        _viewModel.OnCloseRequested += OnCloseRequested;

        // Header
        Bind(_viewModel.StageTypeName, text => _txtStageType.text = text);
        Bind(_viewModel.StageName, text => _txtStageName.text = text);

        // Sub-UI ViewModel 바인딩
        if (_weakCodeInfo != null)
            _weakCodeInfo.SetViewModel(_viewModel.WeakCodeInfo);
        if (_rangeInfo != null)
            _rangeInfo.SetViewModel(_viewModel.RangeInfo);

        // Rapture Icons
        InitRaptureIcons();

        // Close Button
        if (_btnClose != null)
        {
            _btnClose.onClick.RemoveAllListeners();
            _btnClose.onClick.AddListener(() =>
            {
                _viewModel.RequestClose();
            });
        }
    }

    private async void InitRaptureIcons()
    {
        ClearIcons();

        // Target (Grade 3)
        await CreateIcons(_viewModel.TargetIconViewModels, _targetIconContainer);

        // SubTarget (Grade 2)
        await CreateIcons(_viewModel.SubTargetIconViewModels, _subTargetIconContainer);

        // Soldier (Grade 1)
        await CreateIcons(_viewModel.SoldierIconViewModels, _soldierIconContainer);
    }

    private async Task CreateIcons(List<RaptureIconViewModel> viewModels, Transform container)
    {
        if (container == null) return;

        foreach (var vm in viewModels)
        {
            var icon = await Managers.UI.ShowAsync<UI_RaptureIcon>(vm, container);
            if (icon != null)
                _spawnedIcons.Add(icon);
        }
    }

    private void ClearIcons()
    {
        foreach (var icon in _spawnedIcons)
        {
            if (icon != null)
                Managers.UI.Close(icon);
        }
        _spawnedIcons.Clear();
    }

    private void OnCloseRequested()
    {
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnCloseRequested -= OnCloseRequested;
        }

        ClearIcons();
        base.OnDestroy();
    }
}

