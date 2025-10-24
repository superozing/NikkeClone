using System;
using UI;
using UnityEngine;

public class ItemIconViewModel : IIconViewModel, IDisposable
{
    public event Action OnStateChanged;

    private readonly ItemGameData _gameData;
    private readonly UserItemData _userData;

    public string MainIconAddress => _gameData?.iconPath;
    public string RarityFrameAddress => null; // 아이템은 희귀도 프레임을 사용하지 않을 생각이에요.
    public string QuantityText => "X " + Utils.FormatNumber(_userData.count.Value); // 앞에 X를 붙여서 개수임을 나타내요.

    /// <summary>
    /// 표시할 아이템의 eItemType을 받아 ViewModel을 생성합니다.
    /// </summary>
    /// <param name="itemType">표시할 아이템의 eItemType</param>
    public ItemIconViewModel(eItemType itemType)
    {
        int itemID = (int)itemType;

        _gameData = Managers.Data.Get<ItemGameData>(itemID);
        if (_gameData == null)
            Debug.LogError($"[ItemIconViewModel] ID({itemID}, {itemType})에 해당하는 ItemGameData를 찾을 수 없습니다.");

        if (!Managers.Data.UserData.Items.TryGetValue(itemID, out _userData))
            Debug.LogError($"[ItemIconViewModel] ID({itemID}, {itemType})에 해당하는 UserItemData를 찾을 수 없습니다.");

        _userData.count.OnValueChanged += OnValueChanged;
    }

    private void OnValueChanged(int _) => OnStateChanged?.Invoke();

    public void Dispose()
    {
        if (_userData != null)
            _userData.count.OnValueChanged -= OnValueChanged;
    }
}