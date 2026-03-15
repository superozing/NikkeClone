using System;
using System.Threading.Tasks;
using UI;

/// <summary>
/// 전투 일시정지 팝업의 전체 로직을 담당하는 ViewModel입니다.
/// </summary>
public class CombatPausePopupViewModel : ViewModelBase
{
    public ReactiveProperty<string> TimeText { get; } = new();
    public NikkeCombatSlotViewModel[] SlotViewModels { get; } = new NikkeCombatSlotViewModel[5];

    private readonly CombatStatRecordSystem _statRecordSystem;
    private readonly CombatNikke[] _nikkes;

    public CombatPausePopupViewModel(string timeText, CombatStatRecordSystem statRecordSystem, CombatNikke[] nikkes)
    {
        TimeText.Value = timeText;
        _statRecordSystem = statRecordSystem;
        _nikkes = nikkes;

        // 1. 슬롯 VM 생성 (인라인)
        for (int i = 0; i < 5; i++)
        {
            if (i < _nikkes.Length && _nikkes[i] != null)
            {
                var record = _statRecordSystem.GetRecord(i);
                int slotIndex = i;

                var slotVM = new NikkeCombatSlotViewModel(_nikkes[i], record, () => OnSlotClicked(slotIndex));
                SlotViewModels[i] = slotVM;
                SlotViewModels[i].AddRef();
            }
        }

        // 2. 팀 단위 최대값 기준 Ratio 계산 및 주입
        long maxDamageDealt = 0;
        long maxDamageTaken = 0;
        long maxHealReceived = 0;

        foreach (var vm in SlotViewModels)
        {
            if (vm == null) continue;
            if (vm.DamageDealt.Value > maxDamageDealt) maxDamageDealt = vm.DamageDealt.Value;
            if (vm.DamageTaken.Value > maxDamageTaken) maxDamageTaken = vm.DamageTaken.Value;
            if (vm.HealReceived.Value > maxHealReceived) maxHealReceived = vm.HealReceived.Value;
        }

        foreach (var vm in SlotViewModels)
        {
            if (vm == null) continue;
            vm.DamageDealtRatio.Value = maxDamageDealt > 0 ? (float)vm.DamageDealt.Value / maxDamageDealt : 0f;
            vm.DamageTakenRatio.Value = maxDamageTaken > 0 ? (float)vm.DamageTaken.Value / maxDamageTaken : 0f;
            vm.HealReceivedRatio.Value = maxHealReceived > 0 ? (float)vm.HealReceived.Value / maxHealReceived : 0f;
        }
    }

    public void OnResumeClicked(UI_Popup popup)
    {
        Managers.Time.ResumeGame();
        Managers.UI.Close(popup);
    }

    public async void OnRetryClicked(UI_Popup popup)
    {
        Managers.Time.ResumeGame();
        Managers.UI.Close(popup);

        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CombatScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    public async void OnEndCombatClicked(UI_Popup popup)
    {
        Managers.Time.ResumeGame();
        Managers.UI.Close(popup);

        // 전투 데이터 정리
        Managers.Data.UserData.Combat = null;

        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _nikkes.Length || _nikkes[slotIndex] == null) return;

        var nikke = _nikkes[slotIndex];
        if (nikke.IsDead) return;

        var slotVM = SlotViewModels[slotIndex];
        var detailVM = new NikkeCombatDetailPopupViewModel(slotVM, nikke);
        _ = Managers.UI.ShowAsync<UI_NikkeCombatDetailPopup>(detailVM);
    }

    protected override void OnDispose()
    {
        foreach (var slotVM in SlotViewModels)
        {
            slotVM?.Release();
        }
        base.OnDispose();
    }
}
