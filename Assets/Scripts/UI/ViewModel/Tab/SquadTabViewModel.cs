using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class SquadTabViewModel : ViewModelBase
{
    // --- Reactive Properties ---
    /// <summary>
    /// ?꾩옱 ?좏깮???ㅼ옘???몃뜳??(0 ~ 4)
    /// </summary>
    public ReactiveProperty<int> CurrentSquadIndex { get; private set; } = new(0);

    /// <summary>
    /// ?꾩옱 ?ㅼ옘?쒖쓽 珥??꾪닾???띿뒪??
    /// </summary>
    public ReactiveProperty<string> TotalCombatPower { get; private set; } = new("0");

    // --- Data ---
    /// <summary>
    /// ?꾩옱 ?좏깮???ㅼ옘?쒖쓽 5媛??щ’???대떦?섎뒗 移대뱶 酉곕え??諛곗뿴.
    /// 鍮??щ’? null?낅땲?? View????諛곗뿴??李몄“?섏뿬 UI瑜?媛깆떊?⑸땲??
    /// </summary>
    public NikkeCardViewModel[] SlotViewModels { get; private set; }

    // --- Caching ---
    /// <summary>
    /// ?ㅼ옘?쒕퀎(5媛? ?щ’蹂?5媛? ViewModel??罹먯떛?섎뒗 2李⑥썝 諛곗뿴?낅땲??
    /// Lazy Caching 諛⑹떇???ъ슜?섎?濡?珥덇린?먮뒗 null?대ŉ, ?묎렐 ???앹꽦?⑸땲??
    /// </summary>
    private NikkeCardViewModel[][] _cachedSquadViewModels = new NikkeCardViewModel[5][];

    private readonly Dictionary<int, UserSquadData> _userSquads;
    private Dictionary<int, Action> _squadEventHandlers = new();

    public SquadTabViewModel()
    {
        // ?좎? ?ㅼ옘???곗씠??李몄“
        _userSquads = Managers.Data.UserData.Squads;

        // 2. 각 스쿼드의 변경 이벤트 구독
        foreach (var kvp in _userSquads)
        {
            int squadId = kvp.Key;

            Action handler = () =>
            {
                int index = squadId - 1;
                if (index < 0 || index >= 5) return;

                // 1. 기존 캐시된 ViewModel들 정리
                if (_cachedSquadViewModels[index] != null)
                {
                    foreach (var vm in _cachedSquadViewModels[index])
                    {
                        if (vm != null)
                        {
                            vm.OnClick -= OnCardViewModelClicked;
                            vm.Release();
                        }
                    }
                    _cachedSquadViewModels[index] = null;
                }

                // 2. 현재 선택된 스쿼드라면 즉시 재생성 및 UI 갱신
                if (CurrentSquadIndex.Value == index)
                {
                    CreateSquadViewModelCache(index);
                    SlotViewModels = _cachedSquadViewModels[index];

                    // 전투력 계산
                    long totalCp = 0;
                    foreach (var vm in SlotViewModels)
                    {
                        if (vm != null)
                            totalCp += vm.CombatPower;
                    }
                    TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

                    // View 강제 갱신 알림
                    CurrentSquadIndex.ForceNotify();
                }
            };

            kvp.Value.OnSlotChanged += handler;
            _squadEventHandlers[squadId] = handler;
        }

        // 기본적으로 0번(첫 번째) 스쿼드를 선택합니다.
        SelectSquad(0);
    }

    /// <summary>
    /// View?먯꽌 ?ㅼ옘???좏깮 踰꾪듉 ?대┃ ???몄텧?⑸땲??
    /// </summary>
    public void OnClickSquadButton(int index)
    {
        if (CurrentSquadIndex.Value == index)
            return;

        SelectSquad(index);
    }

    private void SelectSquad(int index)
    {
        // 1. 罹먯떆 ?뺤씤 諛??앹꽦 (Lazy Loading)
        if (_cachedSquadViewModels[index] == null)
        {
            CreateSquadViewModelCache(index);
        }

        // 2. ?꾩옱 ?щ’ 酉곕え??援먯껜 (罹먯떆??諛곗뿴 李몄“)
        SlotViewModels = _cachedSquadViewModels[index];

        // 3. ?꾪닾??怨꾩궛
        long totalCp = 0;
        foreach (var vm in SlotViewModels)
        {
            if (vm != null)
                totalCp += vm.CombatPower;
        }
        TotalCombatPower.Value = Utils.FormatNumber((int)totalCp);

        // 4. ?몃뜳??蹂寃??뚮┝ (View 媛깆떊 ?몃━嫄?
        // View??OnSquadIndexChanged?먯꽌 SlotViewModels瑜??쎌뼱媛誘濡??쒖꽌媛 以묒슂?⑸땲??
        CurrentSquadIndex.Value = index;
    }

    /// <summary>
    /// ?대떦 ?몃뜳?ㅼ쓽 ?ㅼ옘???곗씠?곕? 濡쒕뱶?섏뿬 ViewModel 罹먯떆瑜??앹꽦?⑸땲??
    /// </summary>
    private void CreateSquadViewModelCache(int index)
    {
        _cachedSquadViewModels[index] = new NikkeCardViewModel[5];

        int squadId = index + 1; // ID??1遺???쒖옉

        if (_userSquads.TryGetValue(squadId, out UserSquadData squadData))
        {
            for (int i = 0; i < 5; i++)
            {
                int nikkeId = squadData.slot[i];

                if (nikkeId != -1) // -1? 鍮??щ’
                {
                    var nikkeGameData = Managers.Data.Get<NikkeGameData>(nikkeId);
                    var userNikkeData = Managers.Data.UserData.Nikkes.ContainsKey(nikkeId) ? Managers.Data.UserData.Nikkes[nikkeId] : null;

                    if (nikkeGameData != null && userNikkeData != null)
                    {
                        var vm = new NikkeCardViewModel(userNikkeData, nikkeGameData);
                        vm.AddRef(); // 罹먯떆?먯꽌 蹂댁쑀?섎?濡?李몄“ 移댁슫??利앷?
                        vm.OnClick += OnCardViewModelClicked;

                        _cachedSquadViewModels[index][i] = vm;
                    }
                }
                else
                {
                    _cachedSquadViewModels[index][i] = null;
                }
            }
        }
    }

    // --- Interaction Methods ---

    public void OnClickSkill(int slotIndex)
    {
        int nikkeId = GetNikkeIdFromCurrentSquad(slotIndex);
        if (nikkeId == -1) return;

        ShowSkillPopup(nikkeId);
    }

    public void OnClickDetail(int slotIndex)
    {
        int nikkeId = GetNikkeIdFromCurrentSquad(slotIndex);
        if (nikkeId == -1) return;

        ShowDetailPopup(nikkeId);
    }

    public void OnClickAutoFormation()
    {
        Debug.Log("[SquadTabViewModel] ?먮룞 ?몄꽦 踰꾪듉 ?대┃??(援ы쁽 ?덉젙)");
    }

    /// <summary>
    /// ?덉? 移대뱶媛 ?대┃?섏뿀?????몄텧?⑸땲??
    /// </summary>
    private async void OnCardViewModelClicked(int nikkeId)
    {
        Debug.Log($"[SquadTabViewModel] 移대뱶 ?대┃?? NikkeID({nikkeId}). UI_SquadDetailPopup ?앹꽦 ?붿껌");

        // 濡쒕뵫 ?앹뾽???듯븳 ?쒖감 ?ㅽ뻾 ?곸슜
        Func<Task> loadTask = async () =>
        {
            // 스쿼드 디테일 팝업은 별도의 데이터 로드 과정없이 생성자에서 처리됨 (가벼운 작업이라 볼 수 있으면 통일성 유지)
            await Managers.UI.ShowAsync<UI_SquadDetailPopup>(new SquadDetailPopupViewModel(CurrentSquadIndex.Value));
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    // --- Helpers ---

    private int GetNikkeIdFromCurrentSquad(int slotIndex)
    {
        // ?꾩옱 ?좏깮???ㅼ옘?쒖쓽 ?곗씠?곗뿉??ID 議고쉶
        if (SlotViewModels != null && slotIndex >= 0 && slotIndex < SlotViewModels.Length)
        {
            var vm = SlotViewModels[slotIndex];
            if (vm != null)
                return vm.NikkeId;
        }
        return -1;
    }

    private async void ShowSkillPopup(int nikkeId)
    {
        SkillInfoPopupViewModel vm = new SkillInfoPopupViewModel();
        vm.SetData(nikkeId);
        await Managers.UI.ShowAsync<UI_SkillInfoPopup>(vm);
    }

    private async void ShowDetailPopup(int nikkeId)
    {
        Func<Task> loadTask = async () =>
        {
            NikkeDetailPopupViewModel vm = new NikkeDetailPopupViewModel();
            await vm.SetNikkeID(nikkeId);
            await Managers.UI.ShowAsync<UI_NikkeDetailPopup>(vm);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    protected override void OnDispose()
    {
        // 스쿼드 이벤트 구독 해제
        if (_userSquads != null)
        {
            foreach (var kvp in _squadEventHandlers)
            {
                if (_userSquads.TryGetValue(kvp.Key, out var squadData))
                {
                    squadData.OnSlotChanged -= kvp.Value;
                }
            }
            _squadEventHandlers.Clear();
        }

        // 캐시된 모든 ViewModel 해제
        if (_cachedSquadViewModels != null)
        {
            for (int i = 0; i < _cachedSquadViewModels.Length; i++)
            {
                var squadVMs = _cachedSquadViewModels[i];
                if (squadVMs != null)
                {
                    foreach (var vm in squadVMs)
                    {
                        if (vm != null)
                        {
                            vm.OnClick -= OnCardViewModelClicked;
                            vm.Release(); // AddRef?????Release
                        }
                    }
                }
                _cachedSquadViewModels[i] = null;
            }
            _cachedSquadViewModels = null;
        }

        SlotViewModels = null;
    }
}