using System;
using System.Collections.Generic;
using UnityEngine;

public class MissionSystem : IDisposable
{
    private UserDataModel _userData;
    private IReadOnlyDictionary<int, MissionGameData> _missionGameData;
    private Dictionary<int, UserMissionData> _userMissions;

    public void Init()
    {
        Debug.Log("[MissionSystem] Init() 합니다.");

        // 1. DataManager 전역 참조 및 캐싱
        // 2. 데이터 참조
        _userData = Managers.Data.UserData;
        _missionGameData = Managers.Data.GetTable<MissionGameData>();
        _userMissions = _userData?.Missions;

        if (_userData == null || _missionGameData == null || _userMissions == null)
        {
            Debug.LogError("[MissionSystem] 데이터 참조에 실패했습니다.");
            return;
        }

        // 3. 유저 데이터 동기화
        foreach (var gameData in _missionGameData.Values)
        {
            if (!_userMissions.ContainsKey(gameData.id))
                _userMissions.Add(gameData.id, new UserMissionData(gameData.id));
        }

        // 4. 게임 내 이벤트 구독 시작
        // 이후 스테이지 데이터 역시 구독해야 해요. 또는 미션시스템에게 직접 변경을 호출할 수도 있겠지.
        // 음... 모든 니케 데이터에 바인딩 하는 게 맞는걸까?
        // 그냥 함수 호출하도록 하는 것이 좋지 않을까?
        foreach (var nikke in _userData.Nikkes.Values)
            nikke.level.OnValueChanged += OnNikkeLevelChanged;

    }

    /// <summary>
    /// 스테이지 클리어를 미션 시스템에 알립니다.
    /// </summary>
    public void ReportStageClear() => UpdateMissionProgress(eMissionType.StageClear, 1);
    private void OnNikkeLevelChanged(int newLevel) => UpdateMissionProgress(eMissionType.NikkeLevelUp, 1);

    /// <summary>
    /// 특정 미션 타입에 대해 진행도를 증가시킵니다.
    /// </summary>
    /// <param name="type">갱신할 미션 타입</param>
    /// <param name="amount">증가량</param>
    private void UpdateMissionProgress(eMissionType type, int amount)
    {
        if (_userMissions == null || _missionGameData == null)
            return;

        foreach (var userMission in _userMissions.Values)
        {
            // 이미 완료된 경우 continue
            if (userMission.state.Value != eMissionState.InProgress)
                continue;

            if (_missionGameData.TryGetValue(userMission.id, out MissionGameData gameData))
            {
                // 미션 타입이 다를 경우 continue
                if (gameData.missionType != type)
                    continue;

                userMission.currentCount.Value += amount;

                if (userMission.currentCount.Value >= gameData.targetCount)
                {
                    userMission.state.Value = eMissionState.Completed;
                    Debug.Log($"[MissionSystem] 미션 완료: {gameData.title}");
                }
            }
        }
    }

    /// <summary>
    /// 미션의 보상을 수령합니다.
    /// </summary>
    /// <param name="missionID">보상 받을 미션 ID</param>
    /// <returns>보상 수령 성공 여부</returns>
    public bool ClaimMissionReward(int missionID)
    {
        if (!_userMissions.TryGetValue(missionID, out UserMissionData userMission))
        {
            Debug.LogError($"[MissionSystem] ID({missionID})에 해당하는 UserMissionData가 없습니다.");
            return false;
        }
        if (!_missionGameData.TryGetValue(missionID, out MissionGameData gameData))
        {
            Debug.LogError($"[MissionSystem] ID({missionID})에 해당하는 MissionGameData가 없습니다.");
            return false;
        }

        // 1. 상태 확인
        if (userMission.state.Value != eMissionState.Completed)
        {
            if (userMission.state.Value == eMissionState.InProgress)
                Debug.Log($"[MissionSystem] ID({missionID}) 미션이 아직 완료되지 않았습니다.");
            else if (userMission.state.Value == eMissionState.RewardClaimed)
                Debug.Log($"[MissionSystem] ID({missionID}) 미션은 이미 보상을 받았습니다.");
            return false;
        }

        // 2. 보상 지급
        if (!Managers.Data.UserData.Items.TryGetValue(gameData.rewardItemID, out UserItemData userItem))
        {
            Debug.LogError($"[MissionSystem] ID({gameData.rewardItemID})에 해당하는 UserItemData가 없습니다.");
            return false;
        }

        userItem.count.Value += gameData.rewardItemCount;
        Debug.Log($"[MissionSystem] 보상 지급 완료: MissionID({missionID}), ItemID({gameData.rewardItemID}), Count({gameData.rewardItemCount})");

        // 3. 상태 변경
        // 변경 시 상태 구독한 UI 쪽에서 RewardPopup 생성해야 해요.
        userMission.state.Value = eMissionState.RewardClaimed;

        return true;
    }

    /// <summary>
    /// DataManager 클리어 시점에 호출되어야 한다.
    /// </summary>
    public void Dispose()
    {
        // 1. 구독했던 모든 이벤트 해제
        if (_userData?.Nikkes != null)
            foreach (var nikke in _userData.Nikkes.Values)
                nikke.level.OnValueChanged -= OnNikkeLevelChanged;

        // 2. 참조 해제
        _userData = null;
        _missionGameData = null;
        _userMissions = null;
    }
}