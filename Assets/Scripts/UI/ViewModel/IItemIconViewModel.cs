using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class ItemIconViewModel : IIconViewModel, IDisposable
{
    public event Action OnStateChanged;

    private ItemGameData _gameData;
    private UserItemData _userData;

    public Sprite MainIconSprite { get; private set; }

    // --- 사용하지 않을 스프라이트 --- 
    public Sprite RarityFrameSprite { get; private set; } = null;
    // ------------------------------

    public string QuantityText
    {
        get
        {
            if (_userData == null)
                return "X 0";
            return "X " + Utils.FormatNumber(_userData.count.Value);
        }
    }

    /// <summary>
    /// ViewModel에 새로운 아이템 타입을 설정합니다.
    /// </summary>
    /// <param name="itemType"></param>
    public async Task SetItem(eItemType itemType)
    {
        // 1. 기존 데이터 구독 해제
        if (_userData != null)
            _userData.count.OnValueChanged -= OnValueChanged;

        // 2. 아이템 게임 데이터와 아이템 유저 데이터 세팅
        int itemID = (int)itemType;
        _gameData = Managers.Data.Get<ItemGameData>(itemID);
        if (!Managers.Data.UserData.Items.TryGetValue(itemID, out _userData))
            _userData = null;
        else
            _userData.count.OnValueChanged += OnValueChanged;

        // 3. 리소스 비동기 로드
        MainIconSprite = await Managers.Resource.LoadAsync<Sprite>(_gameData.iconPath);

        // 4. View 갱신
        OnStateChanged?.Invoke();
    }

    private void OnValueChanged(int _) => OnStateChanged?.Invoke();

    public void Dispose()
    {
        if (_userData != null)
            _userData.count.OnValueChanged -= OnValueChanged;
    }
}