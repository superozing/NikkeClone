using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class MissionButtonViewModel : ViewModelBase
{
    public event Action OnRequestMissionPopup;

    /// <summary>
    /// View에 표시될 현재 미션 설명 텍스트입니다.
    /// </summary>
    public ReactiveProperty<string> MissionDesc { get; private set; } = new("");

    /// <summary>
    /// View에 표시될 빨간 점의 활성화 여부입니다.
    /// </summary>
    public ReactiveProperty<bool> IsRedDotActive { get; private set; } = new(false);

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
    }

    public void OnMissionButtonClicked()
    {
        Debug.Log("미션 버튼 클릭");

        // 빨간 점 비활성화
        if (IsRedDotActive.Value)
            IsRedDotActive.Value = false;

        // 팝업 생성
        OnRequestMissionPopup?.Invoke();
    }

    private void UpdateMissionStatus()
    {
        // 기본값 설정
        string missionDesc = "미션 완료";
        bool isRedDotActive = false;

        foreach (UserMissionData userMission in _userMissions.Values)
        {
            // 보상을 받을 미션이 있다면 빨간점 활성화
            if (userMission.state.Value == eMissionState.Completed)
                isRedDotActive = true;
            // 진행중인 미션이 있다면 설명 설정
            else if (userMission.state.Value == eMissionState.InProgress)
            {
                if (_missionGameData.TryGetValue(userMission.id, out MissionGameData gameData))
                    missionDesc = gameData.description;
            }
        }

        // 값 변경 시에만 할당 (ReactiveProperty 최적화)
        if (MissionDesc.Value != missionDesc) MissionDesc.Value = missionDesc;
        if (IsRedDotActive.Value != isRedDotActive) IsRedDotActive.Value = isRedDotActive;
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
