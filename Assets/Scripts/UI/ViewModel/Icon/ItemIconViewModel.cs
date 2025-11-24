using System.Threading.Tasks;
using UI;
using UnityEngine;

public class ItemIconViewModel : IconViewModel // 상속 변경
{
    private ItemGameData _gameData;
    private UserItemData _userData;

    public override ReactiveProperty<Sprite> MainIconSprite { get; } = new();

    // --- 사용하지 않을 스프라이트 --- 
    public override ReactiveProperty<Sprite> RarityFrameSprite { get; } = new();
    // ------------------------------

    public override ReactiveProperty<string> QuantityText { get; } = new();

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
        {
            _userData.count.OnValueChanged += OnValueChanged;
            // 초기값 설정
            OnValueChanged(_userData.count.Value);
        }

        if (_userData == null)
        {
            QuantityText.Value = "X 0";
        }

        // 3. 리소스 비동기 로드
        if (_gameData != null)
            MainIconSprite.Value = await Managers.Resource.LoadAsync<Sprite>(_gameData.iconPath);
    }

    private void OnValueChanged(int count)
    {
        QuantityText.Value = "X " + Utils.FormatNumber(count);
    }

    // 아이템 팝업에서 아이콘의 버튼 입력 동작은 없다.
    public override void OnClickButton() { }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.count.OnValueChanged -= OnValueChanged;
    }
}
