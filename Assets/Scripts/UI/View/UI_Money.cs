using UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Money : UI_View
{
    [Header("Components")]
    [SerializeField] private TMP_Text _jewelCountText;
    [SerializeField] private TMP_Text _creditCountText;

    [Header("Buttons")]
    [SerializeField] private Button _jewelButton;
    [SerializeField] private Button _creditButton;

    private MoneyViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        // 각 버튼 클릭 시 해당하는 eItemType을 ViewModel의 OnClickItem 메서드로 전달합니다.
        _jewelButton.onClick.AddListener(() => _viewModel?.OnClickItem(eItemType.Jewel));
        _creditButton.onClick.AddListener(() => _viewModel?.OnClickItem(eItemType.Credit));
    }

    /// <summary>
    /// 외부에서 생성된 ViewModel을 주입받고,
    /// 해당 ViewModel 타입으로 캐스팅하여 멤버 변수에 저장하며 데이터 바인딩을 설정합니다.
    /// </summary>
    /// <param name="viewModel">[이 View에서는 반드시 MoneyViewModel과 대응되어야 합니다.] 주입할 ViewModel입니다.</param>
    public override void SetViewModel(ViewModelBase viewModel)
    {
        if (_viewModel != null)
            _viewModel.OnRequestItemDetail -= ShowItemDetailPopup;

        _viewModel = viewModel as MoneyViewModel;

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_Money] 잘못된 ViewModel 타입이 주입되었습니다. Expected: MoneyViewModel, Actual: {viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            _viewModel.OnRequestItemDetail += ShowItemDetailPopup;

            Bind(_viewModel.JewelCountText, text => _jewelCountText.text = text);
            Bind(_viewModel.CreditCountText, text => _creditCountText.text = text);
        }
    }

    private async void ShowItemDetailPopup(eItemType itemType)
    {
        ItemDetailPopupViewModel viewModel = new();
        await viewModel.SetItem(itemType);
        await Managers.UI.ShowAsync<UI_ItemDetailPopup>(viewModel);
    }

    /// <summary>
    /// Unity OnDestroy 생명 주기. ViewModel의 구독을 해제합니다.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 뷰모델 리소스 정리
        _viewModel?.Dispose();
    }
}