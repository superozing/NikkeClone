using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class MissionPopupViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;
    public event Action OnCloseRequested;

    /// <summary>
    /// View가 자식 View에 바인딩 할 ViewModel 리스트
    /// </summary>
    public List<MissionSlotViewModel> SlotViewModels { get; private set; }
    public string MissionResetTimeText { get; private set; }
    public string MissionCompleteTimerText { get; private set; }
    public bool IsAllMissionsComplete { get; private set; }

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
            SlotViewModels.Add(new MissionSlotViewModel(mission.id));
            
            // 이게 모든 미션을 완료했는 지 체크하기 위한 방법으로 올바른 방법인가?
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
    /// 미션 상태 변경 시 모든 미션을 완료했는 지 체크합니다. 이게 맞는 방식일까?
    /// </summary>
    /// <param name="_"></param>
    private void OnMissionStateChanged(eMissionState _)
    {
        bool allComplete = Managers.GameSystem.MissionSystem.IsAllMissionsComplete();

        if (IsAllMissionsComplete != allComplete)
        {
            IsAllMissionsComplete = allComplete;
            OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// TimeSystem의 ReactiveProperty<TimeSpan>.OnValueChanged 이벤트 핸들러입니다.
    /// </summary>
    /// <param name="remainingTime">TimeSystem에서 전달받은 남은 시간</param>
    private void OnTimeUpdated(TimeSpan remainingTime)
    {
        // "HH시간 MM분 남음" 포멧
        MissionResetTimeText = $"{remainingTime.Hours:D2}시간 {remainingTime.Minutes:D2}분 남음";

        // "HH:MM::SS" 포맷
        MissionCompleteTimerText = $"{remainingTime.Hours:D2}:{remainingTime.Minutes:D2}::{remainingTime.Seconds:D2}";

        // 이 것도 ReactiveProperty를 사용한다면 수정이 필요하지 않은 멤버까지 세팅하지 않아도 될 것 같은데.
        // 뷰 쪽에서 모든 이벤트 핸들러 하나하나를 만들어야 한다는 귀찮음이 있지만... 더 효율적일 것 같아요.
        // 이 작업이 비용이 큰 작업은 아니니, 비용이 큰 작업이 발생한다면 그 때 분리해보도록 해요.
        OnStateChanged?.Invoke();
    }

    public void Dispose()
    {
        if (Managers.GameSystem?.TimeSystem != null)
            Managers.GameSystem.TimeSystem.RemainingTime.OnValueChanged -= OnTimeUpdated;

        if (_userMissions != null)
        {
            foreach (UserMissionData mission in _userMissions.Values)
                mission.state.OnValueChanged -= OnMissionStateChanged;
        }

        // 자식 ViewModel 해제
        // 불안하네... 여러 곳에서 dispose 호출하는 이게..
        // 자동으로 처리할 수 있는 무언가 필요해요 ..
        if (SlotViewModels != null)
        {
            foreach (MissionSlotViewModel vm in SlotViewModels)
                (vm as IDisposable)?.Dispose();
            SlotViewModels.Clear();
        }

        OnCloseRequested = null;
    }
}