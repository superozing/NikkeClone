using System;
using UI;
using UnityEngine;

public class RewardItemIconViewModel : ViewModelBase, IIconViewModel
{
    public override event Action OnStateChanged;
    public event Action<int, int> OnRequestRewardPopup; // Icon이 아닌 MissionSlot이 팝업을 열어야 해요.

    private readonly MissionGameData _gameData;
    private readonly UserMissionData _userData;
    private readonly ItemGameData _itemGameData;

    // --- IIconViewModel 구현 ---
    public Sprite MainIconSprite { get; private set; }
    public string QuantityText { get; private set; }

    // 사용하지 않을 것들 --------
    public Sprite RarityFrameSprite { get; private set; } = null;
    // --------------------------

    public RewardItemIconViewModel(MissionGameData gameData, UserMissionData userData)
    {
        _gameData = gameData;
        _userData = userData;

        if (_gameData == null || _userData == null)
        {
            Debug.LogError("[RewardItemIconViewModel] GameData 또는 UserData가 null입니다.");
            return;
        }

        // 1. 보상 아이템 정보 로드
        _itemGameData = Managers.Data.Get<ItemGameData>(_gameData.rewardItemID);
        if (_itemGameData == null)
        {
            Debug.LogError($"[RewardItemIconViewModel] ItemGameData({_gameData.rewardItemID})를 찾을 수 없습니다.");
            return;
        }

        // 2. 수량 텍스트 설정
        QuantityText = $"X {Utils.FormatNumber(_gameData.rewardItemCount)}";

        // 3. 아이콘 설정
        LoadIconAsync();

        // 4. 미션 상태 변경 시 UI 갱신
        _userData.state.OnValueChanged += OnStateDataChanged;
    }

    private async void LoadIconAsync()
    {
        if (_itemGameData == null) 
            return;

        MainIconSprite = await Managers.Resource.LoadAsync<Sprite>(_itemGameData.iconPath);
        OnStateChanged?.Invoke();
    }

    /// <summary>
    /// MissionSystem에게 보상 요청
    /// </summary>
    public void OnClickButton() => Managers.GameSystem.MissionSystem.ClaimMissionReward(_gameData.id);

    private void OnStateDataChanged(eMissionState state)
    {
        // 상태가 보상 수령으로 변경된 경우 팝업 생성 요청
        if (state == eMissionState.RewardClaimed)
        {
            OnRequestRewardPopup?.Invoke(_gameData.rewardItemID, _gameData.rewardItemCount);

            // 아이콘과 텍스트 비우기
            MainIconSprite = null;
            QuantityText = null;    
            OnStateChanged?.Invoke();
        }
    }

    protected override void OnDispose()
    {
        if (_userData != null)
            _userData.state.OnValueChanged -= OnStateDataChanged;

        OnRequestRewardPopup = null;
    }
}