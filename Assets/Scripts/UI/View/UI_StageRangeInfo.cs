using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적정 사거리 정보 Sub-UI View입니다.
/// Near/Mid/Far 사거리별로 스쿼드 내 니케 수를 표시합니다.
/// </summary>
public class UI_StageRangeInfo : UI_View
{
    [Header("Near")]
    [SerializeField] private GameObject[] _nearIcons;
    [SerializeField] private GameObject _nearNoMatchMarker;

    [Header("Mid")]
    [SerializeField] private GameObject[] _midIcons;
    [SerializeField] private GameObject _midNoMatchMarker;

    [Header("Far")]
    [SerializeField] private GameObject[] _farIcons;
    [SerializeField] private GameObject _farNoMatchMarker;

    private StageRangeInfoViewModel _viewModel;

    /// <summary>
    /// ViewModel을 설정하고 데이터 바인딩을 수행합니다.
    /// </summary>
    /// <param name="viewModel">바인딩할 StageRangeInfoViewModel</param>
    public void SetViewModel(StageRangeInfoViewModel viewModel)
    {
        _viewModel = viewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 사거리별 바인딩
        Bind(_viewModel.NearCount, count => UpdateRow(_nearIcons, _nearNoMatchMarker, count));
        Bind(_viewModel.MidCount, count => UpdateRow(_midIcons, _midNoMatchMarker, count));
        Bind(_viewModel.FarCount, count => UpdateRow(_farIcons, _farNoMatchMarker, count));
    }

    /// <summary>
    /// 행별 아이콘 및 X 마커를 업데이트합니다.
    /// </summary>
    /// <param name="icons">업데이트할 아이콘 배열 (5개)</param>
    /// <param name="noMatchMarker">매칭 없을 때 표시할 X 마커</param>
    /// <param name="count">매칭된 니케 수 (0~5)</param>
    private void UpdateRow(GameObject[] icons, GameObject noMatchMarker, int count)
    {
        // 아이콘 활성화 (count만큼만 활성화)
        if (icons != null)
        {
            for (int i = 0; i < icons.Length; i++)
            {
                if (icons[i] != null)
                    icons[i].SetActive(i < count);
            }
        }

        // X 마커 (매칭 니케 없을 때만 표시)
        if (noMatchMarker != null)
            noMatchMarker.SetActive(count == 0);
    }
}
