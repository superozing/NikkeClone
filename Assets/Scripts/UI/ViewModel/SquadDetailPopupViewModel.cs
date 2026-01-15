using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class SquadDetailPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;
    public event Action OnSquadDataChanged; // ?щ’ UI 媛깆떊??

    // --- State ---
    public ReactiveProperty<int> CurrentSquadIndex { get; private set; } = new(0);
    public ReactiveProperty<string> TotalCombatPower { get; private set; } = new("0");

    // --- Child ViewModels ---
    public NikkeIconViewModel[] SlotViewModels { get; private set; }

    public SquadDetailNikkeCardScrollViewModel ScrollViewModel { get; private set; }

    // --- Data ---
    // ?몄쭛 以묒씤 ?꾩떆 ?곗씠??(?ㅼ옘??ID蹂?
    private Dictionary<int, UserSquadData> _tempSquads = new();

    public SquadDetailPopupViewModel(int initialSquadIndex = 0)
    {
        // 1. ?꾩떆 ?곗씠???앹꽦 (Deep Copy)
        CloneSquadData();

        // 2. ?щ’ 酉곕え??珥덇린??
        SlotViewModels = new NikkeIconViewModel[5];
        for (int i = 0; i < 5; i++)
        {
            SlotViewModels[i] = new NikkeIconViewModel();
            SlotViewModels[i].AddRef();
        }

        // 3. ?ㅽ겕濡?酉곕え??珥덇린??
        ScrollViewModel = new SquadDetailNikkeCardScrollViewModel();
        ScrollViewModel.AddRef();
        ScrollViewModel.OnSquadChanged += OnScrollSelectionChanged;

        // ?ㅼ옘???몃뜳?ㅺ? 蹂寃쎈맆 ?뚮쭔 ?щ’??媛깆떊?섎룄濡??대깽??援щ룆
        CurrentSquadIndex.OnValueChanged += OnSquadIndexChanged;

        // 4. 珥덇린 ?ㅼ옘???ㅼ젙
        SelectSquad(initialSquadIndex);
    }

    /// <summary>
    /// DataManager???좎? ?ㅼ옘???곗씠?곕? 蹂듭젣?섏뿬 ?꾩떆 ??μ냼??蹂닿??⑸땲??
    /// </summary>
    private void CloneSquadData()
    {
        _tempSquads.Clear();
        var userSquads = Managers.Data.UserData.Squads;

        // 1~5踰??ㅼ옘???쒗쉶
        for (int i = 1; i <= 5; i++)
        {
            if (userSquads.TryGetValue(i, out var originalData))
            {
                // UserSquadData??Clone 硫붿꽌???ъ슜
                _tempSquads.Add(i, originalData.Clone());
            }
            else
            {
                // ?놁쑝硫?鍮??ㅼ옘???앹꽦
                _tempSquads.Add(i, new UserSquadData(i));
            }
        }
    }

    public void SelectSquad(int index)
    {
        index = Mathf.Clamp(index, 0, 4);

        // OnValueChanged -> RefreshSlots ?몄텧
        if (CurrentSquadIndex.Value != index)
            CurrentSquadIndex.Value = index;
        else
        {
            // 珥덇린 ?ㅽ뻾 ???щ’ 媛깆떊
            OnSquadIndexChanged(index);
        }
    }

    private void OnSquadIndexChanged(int index)
    {
        // ?ㅽ겕濡?酉곕え?몄뿉 ?꾩옱 ?몄쭛???꾩떆 ?곗씠??二쇱엯
        int squadId = index + 1;
        if (_tempSquads.TryGetValue(squadId, out var tempSquadData))
        {
            // Requirement 2: ?ㅼ옘???꾪솚 ?쒖뿉??由ъ뒪???ъ젙???섑뻾
            ScrollViewModel.SetSquadData(tempSquadData);
        }

        RefreshSlots();
    }

    private void OnScrollSelectionChanged()
    {
        // ?ㅽ겕濡ㅻ럭 議곗옉(移대뱶 ?대┃ ???쇰줈 ?곗씠?곌? 蹂?덉쑝???щ’ UI 媛깆떊
        RefreshSlots();
    }

    /// <summary>
    /// ?꾩옱 ?좏깮???ㅼ옘???곗씠?곕줈 ?щ’ ?꾩씠肄섎뱾??媛깆떊?⑸땲??
    /// </summary>
    private async void RefreshSlots()
    {
        int squadId = CurrentSquadIndex.Value + 1;
        if (!_tempSquads.TryGetValue(squadId, out var currentSquadData))
            return;

        var currentSlots = currentSquadData.slot;
        long totalCp = 0;

        for (int i = 0; i < 5; i++)
        {
            int nikkeId = currentSlots[i];
            await SlotViewModels[i].SetNikke(nikkeId);

            // ?꾪닾???⑹궛
            if (nikkeId != -1 && Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userNikke))
            {
                totalCp += userNikke.combatPower.Value;
            }
        }

        TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

        // 酉?媛깆떊 ?뚮┝
        OnSquadDataChanged?.Invoke();
    }

    /// <summary>
    /// ?쒕옒洹????쒕∼?쇰줈 ?щ’ 媛??꾩튂瑜?援먰솚?섍굅???대룞?⑸땲??
    /// </summary>
    public void SwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= 5 || toIndex < 0 || toIndex >= 5) return;

        int squadId = CurrentSquadIndex.Value + 1;
        var slots = _tempSquads[squadId].slot;

        int temp = slots[fromIndex];
        slots[fromIndex] = slots[toIndex];
        slots[toIndex] = temp;

        // ?곗씠??蹂寃????곷떒 ?꾩씠肄?媛깆떊 (?꾨씫 ?섏젙)
        RefreshSlots();

        // ?ㅽ겕濡ㅻ럭???좏깮 ?곹깭 媛깆떊 (Requirement 1: ?ъ젙???덊븿)
        ScrollViewModel.UpdateSelectionState(sort: false);
    }

    public void RemoveNikkeFromSlot(int slotIndex)
    {
        int squadId = CurrentSquadIndex.Value + 1;
        if (!_tempSquads.TryGetValue(squadId, out var currentSquadData))
            return;

        if (slotIndex < 0 || slotIndex >= 5) return;

        // 1. ?곗씠???섏젙
        currentSquadData.slot[slotIndex] = -1;

        // 2. ?대떦 ?щ’ ViewModel留?媛깆떊 (?꾩껜 RefreshSlots ?몄텧 ?덊븿)
        // 鍮꾨룞湲??몄텧?댁?留?寃곌낵瑜?湲곕떎由ъ? ?딄퀬 吏꾪뻾 (Fire and Forget)
        _ = SlotViewModels[slotIndex].SetNikke(-1);

        // ?꾩껜 CP ?ш퀎?곗씠 ?꾩슂?섎?濡?RefreshSlots瑜??몄텧?섏뿬 CP? ?곹깭瑜?留욎땅?덈떎.
        RefreshSlots();

        // 3. ?ㅽ겕濡?酉??좏깮 ?곹깭 媛깆떊 (Requirement 1: ?ъ젙???덊븿)
        ScrollViewModel.UpdateSelectionState(sort: false);
    }

    public async void ShowNikkeDetail(int slotIndex)
    {
        int squadId = CurrentSquadIndex.Value + 1;
        if (!_tempSquads.TryGetValue(squadId, out var currentSquadData))
            return;

        if (slotIndex < 0 || slotIndex >= 5) return;

        int nikkeId = currentSquadData.slot[slotIndex];
        if (nikkeId == -1) return; // 鍮??щ’

        Func<Task> loadTask = async () =>
        {
            NikkeDetailPopupViewModel popupVM = new NikkeDetailPopupViewModel();
            await popupVM.SetNikkeID(nikkeId);
            await Managers.UI.ShowAsync<UI_NikkeDetailPopup>(popupVM);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    public void OnClickAutoFormation()
    {
        // ?먮룞 ?몄꽦 濡쒖쭅 (湲곗〈怨??좎궗?섎굹 ?꾩떆 ?곗씠???ъ슜)
        var userNikkes = Managers.Data.UserData.Nikkes.Values.ToList();
        var gameDataTable = Managers.Data.GetTable<NikkeGameData>();

        var sortedCandidates = userNikkes
            .Select(u => new { User = u, Game = gameDataTable[u.id] })
            .OrderByDescending(x => x.User.combatPower.Value)
            .ToList();

        HashSet<int> usedIds = new HashSet<int>();
        List<int> newSlots = new List<int> { -1, -1, -1, -1, -1 };

        int PickBest(int burstLevel)
        {
            foreach (var candidate in sortedCandidates)
            {
                if (usedIds.Contains(candidate.User.id)) continue;
                if (burstLevel == 0 || candidate.Game.burstLevel == burstLevel)
                {
                    usedIds.Add(candidate.User.id);
                    return candidate.User.id;
                }
            }
            return -1;
        }

        newSlots[0] = PickBest(1);
        newSlots[1] = PickBest(2);
        newSlots[2] = PickBest(3);
        newSlots[3] = PickBest(3);
        newSlots[4] = PickBest(0);

        for (int i = 0; i < 5; i++)
        {
            if (newSlots[i] == -1) newSlots[i] = PickBest(0);
        }

        int squadId = CurrentSquadIndex.Value + 1;
        _tempSquads[squadId].slot = newSlots;

        // ?꾩씠肄?媛깆떊
        RefreshSlots();

        // ?ㅽ겕濡ㅻ럭 媛깆떊 (Requirement 1: ?ъ젙???덊븿)
        ScrollViewModel.UpdateSelectionState(sort: false);

        Debug.Log("[SquadDetail] ?먮룞 ?몄꽦 ?꾨즺");
    }

    public void OnClickReset()
    {
        int squadId = CurrentSquadIndex.Value + 1;
        var slots = _tempSquads[squadId].slot;
        for (int i = 0; i < 5; i++) slots[i] = -1;

        // ?꾩씠肄?媛깆떊
        RefreshSlots();

        // ?ㅽ겕濡ㅻ럭 媛깆떊 (Requirement 1: ?ъ젙???덊븿)
        ScrollViewModel.UpdateSelectionState(sort: false);
    }

    public void OnClickSave()
    {
        // 1. ?꾩떆 ?곗씠?곕? ?ㅼ젣 UserData??諛섏쁺 (Overwrite)
        var realSquads = Managers.Data.UserData.Squads;

        foreach (var kvp in _tempSquads)
        {
            int id = kvp.Key;
            UserSquadData tempData = kvp.Value;

            if (realSquads.TryGetValue(id, out var userSquad))
            {
                // 由ъ뒪???댁슜 蹂듭궗
                userSquad.slot = new List<int>(tempData.slot);
                // 변경 알림 발생
                userSquad.NotifySlotChanged();
            }
            else
            {
                // ?놁쑝硫??덈줈 ?앹꽦?섏뿬 異붽?
                realSquads.Add(id, tempData.Clone());
            }
        }

        // 2. 濡쒖뺄 ???
        Managers.Data.SaveUserData();

        Debug.Log("[SquadDetail] ?ㅼ옘??蹂寃쎌궗??????꾨즺.");
        OnCloseRequested?.Invoke();
    }

    public void OnClickClose()
    {
        // ??ν븯吏 ?딄퀬 ?レ쓬 (?꾩떆 ?곗씠???뚭린??
        OnCloseRequested?.Invoke();
    }

    protected override void OnDispose()
    {
        if (CurrentSquadIndex != null)
            CurrentSquadIndex.OnValueChanged -= OnSquadIndexChanged;

        if (SlotViewModels != null)
        {
            foreach (var vm in SlotViewModels) vm.Release();
            SlotViewModels = null;
        }

        if (ScrollViewModel != null)
        {
            ScrollViewModel.OnSquadChanged -= OnScrollSelectionChanged;
            ScrollViewModel.Release();
            ScrollViewModel = null;
        }

        OnCloseRequested = null;
        OnSquadDataChanged = null;
    }
}