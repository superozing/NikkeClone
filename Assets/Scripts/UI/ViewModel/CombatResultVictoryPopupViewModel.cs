using System.Collections.Generic;
using UI;
using UnityEngine;

public class CombatResultVictoryPopupViewModel : ViewModelBase
{
    // View에서 바인딩할 아이콘 뷰모델 리스트
    public ReactiveProperty<List<StageRewardItemIconViewModel>> RewardItemViewModels { get; private set; } = new(new());

    public CombatResultVictoryPopupViewModel(List<RewardData> rewards)
    {
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

    public void OnScreenClicked()
    {
        // 캠페인 씬으로 돌아가기(이때 비동기로드를 하면 좋겠죠?)
        Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
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
