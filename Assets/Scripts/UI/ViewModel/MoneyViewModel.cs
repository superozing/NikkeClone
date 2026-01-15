using System;
using UnityEngine;

public class MoneyViewModel : ViewModelBase
{
    public event Action<eItemType> OnRequestItemDetail;

    private ReactiveProperty<int> _jewelCountRef;
    private ReactiveProperty<int> _creditCountRef;

    public ReactiveProperty<string> JewelCountText { get; private set; } = new("0");
    public ReactiveProperty<string> CreditCountText { get; private set; } = new("0");

    public MoneyViewModel()
    {
        // 뷰가 데이터를 가져갈 수 있도록 이것저것 초기화 해요.

        // 1. UserData에서 유저 아이템 데이터 참조
        if (Managers.Data.UserData == null)
            Debug.LogError("[MoneyViewModel] UserData가 null입니다.");

        if (Managers.Data.UserData.Items.TryGetValue((int)eItemType.Jewel, out UserItemData jewelItem))
            _jewelCountRef = jewelItem.count;
        if (Managers.Data.UserData.Items.TryGetValue((int)eItemType.Credit, out UserItemData creditItem))
            _creditCountRef = creditItem.count;

        // 초기 문자열 값 설정
        UpdateTextProperties();

        // 2. 이벤트 구독
        // 값 변경 시 View에 상태 변경 호출해요.
        if (_jewelCountRef != null) _jewelCountRef.OnValueChanged += OnDataChanged;
        if (_creditCountRef != null) _creditCountRef.OnValueChanged += OnDataChanged;
    }

    /// <summary>
    /// Item 버튼 클릭 시 호출될 콜백입니다. 해당 Item Popup Modal을 생성합니다.
    /// </summary>
    /// <param name="itemType">클릭된 아이템의 타입입니다.</param>
    public void OnClickItem(eItemType itemType)
    {
        Debug.Log($"OnClickItem() 호출됨: {itemType}");
        OnRequestItemDetail?.Invoke(itemType);
    }

    private void OnDataChanged(int newValue)
    {
        UpdateTextProperties();
    }

    private void UpdateTextProperties()
    {
        if (_jewelCountRef != null) JewelCountText.Value = _jewelCountRef.Value.ToString();
        if (_creditCountRef != null) CreditCountText.Value = Utils.FormatNumber(_creditCountRef.Value);
    }

    protected override void OnDispose()
    {
        if (_jewelCountRef != null)
            _jewelCountRef.OnValueChanged -= OnDataChanged;
        if (_creditCountRef != null)
            _creditCountRef.OnValueChanged -= OnDataChanged;

        OnRequestItemDetail = null;
    }
}