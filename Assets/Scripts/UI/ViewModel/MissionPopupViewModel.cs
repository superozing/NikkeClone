using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class MissionPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;

    /// <summary>
    /// View가 자식 View에 바인딩 할 ViewModel 리스트
    /// </summary>
    public List<MissionSlotViewModel> SlotViewModels { get; private set; }
    public ReactiveProperty<string> MissionResetTimeText { get; private set; } = new("");
    public ReactiveProperty<string> MissionCompleteTimerText { get; private set; } = new("");
    public ReactiveProperty<bool> IsAllMissionsComplete { get; private set; } = new(false);

    private readonly Dictionary<int, UserMissionData> _userMissions;

    public MissionPopupViewModel()
    {
        // 1. 데이터 참조
        _userMissions = Managers.Data.UserData.Missions;
        if (_userMissions == null)
        {
            Debug.LogError("[MissionPopupViewModel] UserData.Missions가 null입니다.");
            return;
        }

        // 2. 자식 ViewModel 생성
        SlotViewModels = new List<MissionSlotViewModel>(_userMissions.Count);

        foreach (UserMissionData mission in _userMissions.Values)
        {
            var vm = new MissionSlotViewModel(mission.id);

            vm.AddRef();

            SlotViewModels.Add(vm);

            // 상태 변경 감지
            mission.state.OnValueChanged += OnMissionStateChanged;
        }

        // 3. TimeSystem의 ReactiveProperty 구독
        Managers.GameSystem.TimeSystem.RemainingTime.OnValueChanged += OnTimeUpdated;

        // 4. 초기 상태 설정
        OnTimeUpdated(Managers.GameSystem.TimeSystem.RemainingTime.Value);
        OnMissionStateChanged(default);
    }

    public void OnClose()
    {
        OnCloseRequested?.Invoke();
    }

    /// <summary>
    /// 미션 상태 변경 시 모든 미션을 완료했는 지 체크합니다.
    /// </summary>
    private void OnMissionStateChanged(eMissionState _)
    {
        bool allComplete = Managers.GameSystem.MissionSystem.IsAllMissionsComplete();

        // ReactiveProperty 값 갱신
        if (IsAllMissionsComplete.Value != allComplete)
        {
            IsAllMissionsComplete.Value = allComplete;
        }
    }

    /// <summary>
    /// TimeSystem의 ReactiveProperty<TimeSpan>.OnValueChanged 이벤트 핸들러입니다.
    /// </summary>
    private void OnTimeUpdated(TimeSpan remainingTime)
    {
        // "H시간 M분 남음" 포멧
        MissionResetTimeText.Value = $"{remainingTime.Hours}시간 {remainingTime.Minutes}분 남음";

        // "HH:MM::SS" 포맷
        MissionCompleteTimerText.Value = $"{remainingTime.Hours:D2}:{remainingTime.Minutes:D2}::{remainingTime.Seconds:D2}";
    }

    protected override void OnDispose()
    {
        if (Managers.GameSystem?.TimeSystem != null)
            Managers.GameSystem.TimeSystem.RemainingTime.OnValueChanged -= OnTimeUpdated;

        if (_userMissions != null)
        {
            foreach (UserMissionData mission in _userMissions.Values)
                mission.state.OnValueChanged -= OnMissionStateChanged;
        }

        // 자식 ViewModel 해제 (Dispose -> Release 수정)
        if (SlotViewModels != null)
        {
            foreach (MissionSlotViewModel vm in SlotViewModels)
                vm?.Release();

            SlotViewModels.Clear();
        }

        OnCloseRequested = null;
    }
}