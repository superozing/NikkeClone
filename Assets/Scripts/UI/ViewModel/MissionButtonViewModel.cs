using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class MissionButtonViewModel : ViewModelBase
{
    public override event Action OnStateChanged;
    public event Action OnRequestMissionPopup;

    /// <summary>
    /// View에 표시될 현재 미션 설명 텍스트입니다.
    /// </summary>
    public string MissionDesc { get; private set; }

    /// <summary>
    /// View에 표시될 빨간 점의 활성화 여부입니다.
    /// (완료되었으나 보상받지 않은 미션이 있을 경우 true)
    /// </summary>
    public bool IsRedDotActive { get; private set; }

    private readonly Dictionary<int, UserMissionData> _userMissions;
    private readonly IReadOnlyDictionary<int, MissionGameData> _missionGameData;

    public MissionButtonViewModel()
    {
        _userMissions = Managers.Data.UserData?.Missions;
        _missionGameData = Managers.Data.GetTable<MissionGameData>();

        if (_userMissions == null || _missionGameData == null)
        {
            Debug.LogError("[MissionButtonViewModel] UserData 또는 MissionGameData 로드에 실패했습니다.");
            return;
        }

        foreach (UserMissionData mission in _userMissions.Values)
            mission.state.OnValueChanged += OnMissionStateChanged;

        // 상태 초기화
        UpdateMissionStatus();
    }

    private void OnMissionStateChanged(eMissionState _)
    {
        UpdateMissionStatus();
        OnStateChanged?.Invoke();
    }

    public void OnMissionButtonClicked()
    {
        Debug.Log("미션 버튼 클릭");

        // 빨간 점 비활성화
        if (IsRedDotActive)
        {
            IsRedDotActive = false;
            OnStateChanged?.Invoke();
        }

        // 팝업 생성
        OnRequestMissionPopup?.Invoke();
    }

    private void UpdateMissionStatus()
    {
        // 모든 미션이 완료된 경우
        MissionDesc = "미션 완료";

        foreach (UserMissionData userMission in _userMissions.Values)
        {
            // 보상을 받을 미션이 있다면 빨간점 활성화
            if (userMission.state.Value == eMissionState.Completed)
                IsRedDotActive = true;
            // 진행중인 미션이 있다면 설명 설정
            else if (userMission.state.Value == eMissionState.InProgress)
            {
                if (_missionGameData.TryGetValue(userMission.id, out MissionGameData gameData))
                    MissionDesc = gameData.description;
            }
        }
    }

    protected override void OnDispose()
    {
        if (_userMissions != null)
        {
            foreach (UserMissionData mission in _userMissions.Values)
                mission.state.OnValueChanged -= OnMissionStateChanged;
        }
    }
}