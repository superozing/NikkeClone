using System.Collections.Generic;
using UI;
using UnityEngine;

/// <summary>
/// 스테이지 보상 정보 ViewModel입니다.
/// StageInfoPopupViewModel이 소유하며, 보상 아이콘 ViewModel 배열을 관리합니다.
/// </summary>
public class StageRewardInfoViewModel : ViewModelBase
{
    /// <summary>
    /// 보상 아이콘 ViewModel 목록입니다.
    /// </summary>
    public List<StageRewardItemIconViewModel> RewardIconViewModels { get; private set; } = new List<StageRewardItemIconViewModel>();

    /// <summary>
    /// 활성화할 보상 아이콘 수입니다.
    /// View에서 이 값을 구독하여 아이콘을 활성화/비활성화합니다.
    /// </summary>
    public ReactiveProperty<int> RewardCount { get; private set; } = new(0);

    /// <summary>
    /// 보상 데이터를 설정하고 ViewModel을 업데이트합니다.
    /// 필요한 만큼 ViewModel을 생성하거나 재사용합니다.
    /// </summary>
    /// <param name="rewards">보상 데이터 목록</param>
    public void SetData(List<RewardData> rewards)
    {
        int requiredCount = rewards?.Count ?? 0;

        // 1. 모자란 만큼 추가 생성
        while (RewardIconViewModels.Count < requiredCount)
        {
            var newIconVM = new StageRewardItemIconViewModel();
            newIconVM.AddRef(); // 부모 ViewModel이 자식을 소유
            RewardIconViewModels.Add(newIconVM);
        }

        // 2. 데이터 설정 및 재사용
        for (int i = 0; i < RewardIconViewModels.Count; ++i)
        {
            if (i < requiredCount)
                RewardIconViewModels[i].SetData(rewards[i].itemId, rewards[i].count);
            else
                RewardIconViewModels[i].Clear();
        }

        RewardCount.Value = requiredCount;
    }

    /// <summary>
    /// ViewModel 해제 시 자식 ViewModel들을 Release합니다.
    /// </summary>
    protected override void OnDispose()
    {
        if (RewardIconViewModels != null)
        {
            foreach (var iconVM in RewardIconViewModels)
            {
                iconVM?.Release();
            }
            RewardIconViewModels.Clear();
        }
    }
}
