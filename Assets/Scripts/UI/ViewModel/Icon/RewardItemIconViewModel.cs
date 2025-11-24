using System;
using UI;
using UnityEngine;

public class RewardItemIconViewModel : IconViewModel
{
    public event Action<int, int> OnRequestRewardPopup; // Icon이 아닌 MissionSlot이 팝업을 열어야 해요.

    private readonly MissionGameData _gameData;
    private readonly UserMissionData _userData;

    // --- IconViewModel 구현 ---
    public override ReactiveProperty<Sprite> MainIconSprite { get; } = new();

    // 사용하지 않을 것들 --------
    public override ReactiveProperty<Sprite> RarityFrameSprite { get; } = new();
    // --------------------------

    public override ReactiveProperty<string> QuantityText { get; } = new();

    public RewardItemIconViewModel(MissionGameData gameData, UserMissionData userData)
    {
        _gameData = gameData;
        _userData = userData;

        if (_gameData == null || _userData == null)
        {
            Debug.LogError("[RewardItemIconViewModel] GameData 또는 UserData가 null입니다.");
            return;
        }

        // 1. 초기 텍스트 설정
        QuantityText.Value = $"X {Utils.FormatNumber(_gameData.rewardItemCount)}";

        // 2. 아이콘 설정
        LoadIconAsync();

        // 3. 미션 상태 변경 시 UI 갱신
        _userData.state.OnValueChanged += OnStateDataChanged;
    }

    private async void LoadIconAsync()
    {
        var itemGameData = Managers.Data.Get<ItemGameData>(_gameData.rewardItemID);
        if (itemGameData == null)
        {
            Debug.LogError($"[RewardItemIconViewModel] ItemGameData({_gameData.rewardItemID})를 찾을 수 없습니다.");
            return;
        }

        MainIconSprite.Value = await Managers.Resource.LoadAsync<Sprite>(itemGameData.iconPath);
    }

    /// <summary>
    /// MissionSystem에게 보상 요청
    /// </summary>
    public override void OnClickButton() => Managers.GameSystem.MissionSystem.ClaimMissionReward(_gameData.id);

    private void OnStateDataChanged(eMissionState state)
    {
        // 상태가 보상 수령으로 변경된 경우 팝업 생성 요청
        if (state == eMissionState.RewardClaimed)
        {
            OnRequestRewardPopup?.Invoke(_gameData.rewardItemID, _gameData.rewardItemCount);

            // 아이콘과 텍스트 비우기
            MainIconSprite.Value = null;
            QuantityText.Value = null;
        }
    }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.state.OnValueChanged -= OnStateDataChanged;

        OnRequestRewardPopup = null;
    }
}