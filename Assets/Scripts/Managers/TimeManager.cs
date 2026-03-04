using UnityEngine;

/// <summary>
/// 전투 씬 전역 일시 정지 및 시간 제어를 담당하는 매니저입니다.
/// </summary>
public class TimeManager : IManagerBase
{
    public bool IsPaused { get; private set; }

    public eManagerType ManagerType => eManagerType.Time;

    public void Init()
    {
        IsPaused = false;
        Time.timeScale = 1.0f;
    }

    public void Update()
    {
        // 필요 시 전역 타이머 업데이트 로직 추가 가능
    }

    public void Clear()
    {
        // 씬 전환 시 강제 재개
        ResumeGame();
    }

    /// <summary>
    /// 게임을 일시 정지합니다.
    /// </summary>
    public void PauseGame()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;

        Debug.Log("Game Paused");
    }

    /// <summary>
    /// 게임을 재개합니다.
    /// </summary>
    public void ResumeGame()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1.0f;

        Debug.Log("Game Resumed");
    }

    /// <summary>
    /// 일시 정지 상태를 토글합니다.
    /// </summary>
    public void TogglePause()
    {
        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }
}
