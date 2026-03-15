using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;

public class CombatResultVictoryPopupViewModel : ViewModelBase
{
    //"{Chapter}-{Stage}" 형식의 정보
    public ReactiveProperty<string> StageInfo { get; } = new("");

    // View에서 바인딩할 아이콘 뷰모델 리스트
    public ReactiveProperty<List<StageRewardItemIconViewModel>> RewardItemViewModels { get; private set; } = new(new());

    public CombatResultVictoryPopupViewModel(List<RewardData> rewards, string stageInfo)
    {
        StageInfo.Value = stageInfo;

        var list = new List<StageRewardItemIconViewModel>();
        foreach (var reward in rewards)
        {
            var iconViewModel = new StageRewardItemIconViewModel();
            iconViewModel.SetData(reward.itemId, reward.count);
            iconViewModel.AddRef(); // 자식 뷰모델 참조 카운트 증가
            list.Add(iconViewModel);
        }
        RewardItemViewModels.Value = list;
    }

    public async void OnScreenClicked()
    {
        // 캠페인 씬으로 돌아가기
        // 전투 데이터 정리
        Managers.Data.UserData.Combat = null;

        Func<Task> loadTask = async () =>
        {
            await Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
        };

        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        // 자식 뷰모델 정리
        if (RewardItemViewModels.Value != null)
        {
            foreach (var vm in RewardItemViewModels.Value)
                vm.Release();
            RewardItemViewModels.Value.Clear();
        }
    }
}
