using System;
using System.Threading.Tasks;
using UnityEngine;

public class CombatResultDefeatPopupViewModel : ViewModelBase
{
    public async void OnRetryClicked()
    {
        // 전투 데이터 유지하고 씬 로드
        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CombatScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    public async void OnUpgradeClicked()
    {
        // TODO: MainScene 로드 후 니케 탭으로 이동하도록 해야 해요. 아직 그런 방법이 구현되지 않았어요.
        Debug.Log("[CombatScene] 니케 강화 버튼(니케 탭으로 이동하는 로직이 아직 구현되지 않았어요)");

        // 전투 데이터 정리
        Managers.Data.UserData.Combat = null;

        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.MainScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    public async void OnExitClicked()
    {
        // 전투 데이터 정리
        Managers.Data.UserData.Combat = null;

        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }
}
