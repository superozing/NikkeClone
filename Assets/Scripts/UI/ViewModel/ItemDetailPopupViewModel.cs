using System;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class ItemDetailPopupViewModel : ViewModelBase
{
    public event Action OnClose;

    private ItemGameData _gameData;
    private UserItemData _userData;

    public ItemIconViewModel IconViewModel { get; private set; }

    public ReactiveProperty<string> ItemName { get; private set; } = new("");
    public ReactiveProperty<string> QuantityText { get; private set; } = new("");
    public ReactiveProperty<string> DescText { get; private set; } = new("");

    public ItemDetailPopupViewModel()
    {
        IconViewModel = new ItemIconViewModel();
        IconViewModel.AddRef();
    }

    /// <summary>
    /// 팝업에 표시할 아이템 타입을 설정하고, 현재 뷰모델과 자식 뷰모델을 초기화 합니다.
    /// </summary>
    /// <param name="itemType">표시할 아이템</param>
    public async Task SetItem(eItemType itemType)
    {
        // 1. 기존 데이터 구독 해제
        if (_userData != null)
            _userData.count.OnValueChanged -= OnDataChanged;

        // 2. 새로운 데이터 참조 설정
        int itemID = (int)itemType;
        _gameData = Managers.Data.Get<ItemGameData>(itemID);
        if (!Managers.Data.UserData.Items.TryGetValue(itemID, out _userData))
            _userData = null; // 유저가 해당 아이템을 보유하지 않은 경우

        if (_gameData == null)
        {
            Debug.LogError($"[ItemDetailPopupViewModel] ItemGameData({itemID})를 찾을 수 없습니다.");
            return;
        }

        // 3. 자식 ViewModel(Icon) 초기화
        await IconViewModel.SetItem(itemType);

        // 4. View에 바인딩할 프로퍼티 설정
        ItemName.Value = _gameData.name;
        DescText.Value = _gameData.desc;

        // 5. 새 데이터 구독
        if (_userData != null)
            _userData.count.OnValueChanged += OnDataChanged;

        // 6. View 갱신 통지
        OnDataChanged(default);
    }

    public void OnExit() => OnClose?.Invoke(); // 닫기 이벤트 바인딩

    private void OnDataChanged(int _)
    {
        if (_userData != null)
            QuantityText.Value = $"보유량: {_userData.count.Value}";
        else
            QuantityText.Value = "보유량: 0";
    }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.count.OnValueChanged -= OnDataChanged;

        // 자식 뷰모델도..
        IconViewModel?.Release();
    }
}