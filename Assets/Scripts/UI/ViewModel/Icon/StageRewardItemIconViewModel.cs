using UI;
using UnityEngine;

/// <summary>
/// 개별 보상 아이템 아이콘 ViewModel입니다.
/// StageRewardInfoViewModel이 보유하며, UI_Icon에 바인딩됩니다.
/// </summary>
public class StageRewardItemIconViewModel : IconViewModel
{
    // --- IconViewModel 구현 ---
    public override ReactiveProperty<Sprite> MainIconSprite { get; } = new();
    public override ReactiveProperty<Sprite> RarityFrameSprite { get; } = new(); // 미사용
    public override ReactiveProperty<string> QuantityText { get; } = new();

    private int _itemId;

    /// <summary>
    /// 아이템 데이터를 설정하여 아이콘과 수량 텍스트를 갱신합니다.
    /// </summary>
    /// <param name="itemId">아이템 ID</param>
    /// <param name="count">획득 수량</param>
    public async void SetData(int itemId, int count)
    {
        _itemId = itemId;
        var itemData = Managers.Data.Get<ItemGameData>(itemId);
        if (itemData == null)
        {
            Clear();
            return;
        }

        MainIconSprite.Value = await Managers.Resource.LoadAsync<Sprite>(itemData.iconPath);
        QuantityText.Value = $"x{Utils.FormatNumber(count)}";
    }

    /// <summary>
    /// 빈 상태로 초기화합니다.
    /// </summary>
    public void Clear()
    {
        _itemId = -1;
        MainIconSprite.Value = null;
        QuantityText.Value = null;
    }

    /// <summary>
    /// 아이콘 클릭 시 아이템 정보 팝업을 표시합니다.
    /// </summary>
    public override void OnClickButton()
    {
        // TODO: 아이템 상세 정보 팝업 표시
        if (_itemId > 0)
        {
            Debug.Log($"[StageRewardItemIconViewModel] Clicked item: {_itemId}");
        }
    }
}
