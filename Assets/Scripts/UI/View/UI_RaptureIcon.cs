using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_RaptureIcon : UI_View
{
    [SerializeField] private Image _imgRapture;
    [SerializeField] private Image _imgCodeIcon;
    [SerializeField] private Button _btnClick;

    private RaptureIconViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 버튼 이벤트 해제
        if (_btnClick != null)
        {
            _btnClick.onClick.RemoveListener(OnClick);
        }

        _viewModel = viewModel as RaptureIconViewModel;

        base.SetViewModel(viewModel);

        if (_viewModel == null) return;

        // 이미지 바인딩
        Bind(_viewModel.RaptureSprite, sprite =>
        {
            if (_imgRapture != null)
            {
                _imgRapture.sprite = sprite;
                _imgRapture.enabled = sprite != null;
            }
        });

        Bind(_viewModel.CodeSprite, sprite =>
        {
            if (_imgCodeIcon != null)
            {
                _imgCodeIcon.sprite = sprite;
                _imgCodeIcon.enabled = sprite != null;
            }
        });

        // 버튼 이벤트 연결
        if (_btnClick != null)
        {
            _btnClick.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        _viewModel?.OnClick();
    }
}
