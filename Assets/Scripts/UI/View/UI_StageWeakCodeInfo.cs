using UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 약점 코드 정보 Sub-UI View입니다.
/// 스테이지 약점 코드와 스쿼드 내 매칭 니케 수를 표시합니다.
/// </summary>
public class UI_StageWeakCodeInfo : UI_View
{
    [SerializeField] private GameObject[] _matchedIcons;

    [SerializeField] private Image _CodeImage;

    [SerializeField] private GameObject _noMatchMarker;

    private StageWeakCodeInfoViewModel _viewModel;

    /// <summary>
    /// ViewModel을 설정하고 데이터 바인딩을 수행합니다.
    /// </summary>
    /// <param name="viewModel">바인딩할 StageWeakCodeInfoViewModel</param>
    public void SetViewModel(StageWeakCodeInfoViewModel viewModel)
    {
        _viewModel = viewModel;
        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 약점코드 스프라이트 바인딩
        Bind(_viewModel.WeaknessCodeSprite, sprite =>
        {
            if (_CodeImage != null)
            {
                _CodeImage.sprite = sprite;
                _CodeImage.gameObject.SetActive(sprite != null);
            }
        });

        // 매칭된 니케 수 바인딩
        Bind(_viewModel.MatchedCount, UpdateDisplay);
    }

    private void UpdateDisplay(int matchedCount)
    {
        // 아이콘 활성화 (matchedCount만큼만 활성화)
        if (_matchedIcons != null)
        {
            for (int i = 0; i < _matchedIcons.Length; ++i)
            {
                if (_matchedIcons[i] != null)
                    _matchedIcons[i].SetActive(i < matchedCount);
            }
        }

        // X 마커 (매칭 니케 없을 때만 표시)
        if (_noMatchMarker != null)
            _noMatchMarker.SetActive(matchedCount == 0);
    }
}
