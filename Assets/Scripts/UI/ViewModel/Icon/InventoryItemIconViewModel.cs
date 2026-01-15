using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class InventoryItemIconViewModel : IconViewModel
{
    public event Action<eItemType> OnRequestPopup;

    private ItemGameData _gameData;
    private UserItemData _userData;
    private eItemType _itemType;

    // --- IconViewModel 추상 멤버 구현 ---

    public override ReactiveProperty<Sprite> MainIconSprite { get; } = new();

    // 인벤토리 아이콘에서는 등급 프레임을 사용하지 않음
    public override ReactiveProperty<Sprite> RarityFrameSprite { get; } = new();

    public override ReactiveProperty<string> QuantityText { get; } = new();

    // -----------------------------------

    /// <summary>
    /// 아이템 데이터를 설정하고 리소스를 로드합니다.
    /// </summary>
    public async Task SetItem(eItemType itemType)
    {
        _itemType = itemType;
        int itemID = (int)itemType;

        // 1. 데이터 참조
        _gameData = Managers.Data.Get<ItemGameData>(itemID);

        // 유저 데이터(보유량) 연결
        if (Managers.Data.UserData.Items.TryGetValue(itemID, out _userData))
        {
            _userData.count.OnValueChanged += OnCountChanged;
            OnCountChanged(_userData.count.Value); // 초기값 반영
        }
        else
        {
            // 보유하지 않은 경우 0으로 표시
            QuantityText.Value = "X 0";
        }

        // 2. 아이콘 리소스 로드
        if (_gameData != null)
        {
            MainIconSprite.Value = await Managers.Resource.LoadAsync<Sprite>(_gameData.iconPath);
        }
        else
        {
            Debug.LogError($"[InventoryItemIconViewModel] GameData를 찾을 수 없습니다. ID: {itemID}");
        }
    }

    private void OnCountChanged(int count)
    {
        QuantityText.Value = $"X {Utils.FormatNumber(count)}";
    }

    /// <summary>
    /// 클릭 시 팝업 요청 이벤트를 발생시킵니다.
    /// </summary>
    public override void OnClickButton()
    {
        OnRequestPopup?.Invoke(_itemType);
    }

    protected override void OnDispose()
    {
        // 데이터 구독 해제
        if (_userData != null)
            _userData.count.OnValueChanged -= OnCountChanged;

        OnRequestPopup = null;
    }
}