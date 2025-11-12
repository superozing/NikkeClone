using System;
using UI;
using UnityEngine;

public class MissionSlotViewModel : IViewModel, IDisposable
{
    public event Action OnStateChanged;

    private readonly UserMissionData _userData;
    private readonly MissionGameData _gameData;

    public string Title { get; private set; }
    public string Description { get; private set; }
    public float Progress { get; private set; }
    public string ProgressText { get; private set; }
    public eMissionState MissionState { get; private set; }
    public IIconViewModel RewardIconViewModel { get; private set; } // 보상 아이콘 뷰 모델 새로 생성하고 바인드 해야 해요.

    /// <summary>
    /// ID 값을 받아 미션 뷰모델을 생성합니다.
    /// </summary>
    /// <param name="missionID">참조할 미션의 고유 ID입니다.</param>
    public MissionSlotViewModel(int missionID)
    {
        // 1. DataManager 참조
        _gameData = Managers.Data.Get<MissionGameData>(missionID);
        if (!Managers.Data.UserData.Missions.TryGetValue(missionID, out _userData))
        {
            Debug.LogError($"[MissionSlotViewModel] ID({missionID})에 해당하는 UserMissionData를 찾을 수 없습니다.");
            return;
        }
        if (_gameData == null)
        {
            Debug.LogError($"[MissionSlotViewModel] ID({missionID})에 해당하는 MissionGameData를 찾을 수 없습니다.");
            return;
        }

        // 2. View에 바인딩할 프로퍼티의 초기값 설정
        Title = _gameData.title;
        Description = _gameData.description;

        // 3. 보상 아이콘 뷰모델 생성
        // 자신에게 세팅된 미션을 아이템 아이콘에 전달해요.
        RewardIconViewModel = new RewardItemIconViewModel(_gameData, _userData);

        // 4. 데이터 변경 감지
        _userData.currentCount.OnValueChanged += OnDataChanged;
        _userData.state.OnValueChanged += OnMissionStateChanged;

        // 5. 초기 값 세팅
        OnDataChanged(_userData.currentCount.Value);
        OnMissionStateChanged(_userData.state.Value);
    }

    private void OnMissionStateChanged(eMissionState state)
    {
        MissionState = state;

        OnStateChanged?.Invoke();
    }

    private void OnDataChanged(int _)
    {
        Progress = Mathf.Clamp01((float)_userData.currentCount.Value / _gameData.targetCount);
        ProgressText = $"{Utils.FormatNumber(_userData.currentCount.Value)} / {Utils.FormatNumber(_gameData.targetCount)}";

        OnStateChanged?.Invoke();
    }

    public void Dispose()
    {
        if (_userData != null)
        {
            _userData.currentCount.OnValueChanged -= OnDataChanged;
            _userData.state.OnValueChanged -= OnMissionStateChanged;
        }

        // UI_Icon 쪽에서 호출해주기는 하는데.. 혹시 모르니 Dispose 호출해요.
        (RewardIconViewModel as IDisposable)?.Dispose();
    }
}