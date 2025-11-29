using System;
using System.Collections.Generic;

public class InventoryTabViewModel : ViewModelBase
{
    public event Action<eItemType> OnRequestItemDetail;

    // [추가] 인벤토리 목록 갱신 알림 이벤트
    public event Action OnInventoryUpdated;

    public List<InventoryItemIconViewModel> ItemViewModels { get; private set; } = new();

    public InventoryTabViewModel()
    {
        LoadInventoryData();
    }

    private async void LoadInventoryData()
    {
        var userItems = Managers.Data.UserData.Items;

        if (userItems == null) return;

        // 리스트 초기화 (중복 방지)
        ItemViewModels.Clear();

        foreach (var itemKey in userItems.Keys)
        {
            eItemType itemType = (eItemType)itemKey;

            var iconVM = new InventoryItemIconViewModel();
            iconVM.AddRef();

            // 여기서 await를 만나면 제어권을 반환하므로, View는 아직 빈 리스트를 보게 됨
            await iconVM.SetItem(itemType);

            iconVM.OnRequestPopup += OnChildRequestPopup;

            ItemViewModels.Add(iconVM);
        }

        // 모든 데이터 로드 및 리스트 추가 완료 후 View에게 알림
        OnInventoryUpdated?.Invoke();
    }

    private void OnChildRequestPopup(eItemType itemType)
    {
        OnRequestItemDetail?.Invoke(itemType);
    }

    protected override void OnDispose()
    {
        if (ItemViewModels != null)
        {
            foreach (var vm in ItemViewModels)
            {
                vm.OnRequestPopup -= OnChildRequestPopup;
                vm.Release();
            }
            ItemViewModels.Clear();
            ItemViewModels = null;
        }

        OnRequestItemDetail = null;
        OnInventoryUpdated = null;
    }
}