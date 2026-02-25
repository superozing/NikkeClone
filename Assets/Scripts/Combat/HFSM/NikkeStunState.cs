using UnityEngine;

/// <summary>
/// 기절(CC) 상태. 행동 불가 상태를 처리합니다.
/// 타이머 종료 시 Cover로 복귀하고, EvaluateTransitions()가 후속 판단합니다.
/// </summary>
public class NikkeStunState : IState<CombatNikke>
{
    private float _stunDuration;
    private float _elapsedTime;
    private bool _isFinished;

    /// <summary>
    /// Enter() 호출 전 스턴 지속시간을 주입합니다.
    /// </summary>
    /// Caller: NikkeHFSM.OnStun()
    public void SetStunInfo(float duration)
    {
        _stunDuration = duration;
    }

    public bool IsFinished => _isFinished;

    public void Enter(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Stunned for {_stunDuration}s!");
        _elapsedTime = 0f;
        _isFinished = false;
        owner.View.UpdateVisualState(eNikkeState.Stunned);
    }

    public void Execute(CombatNikke owner)
    {
        _elapsedTime += Time.deltaTime;
        if (_elapsedTime >= _stunDuration)
        {
            _isFinished = true;
            // 전환은 NikkeHFSM.EvaluateTransitions()에서 수행
        }
    }

    public void Exit(CombatNikke owner)
    {
        Debug.Log($"[{owner.name}] Stun Recovered");
        _isFinished = false;
    }
}
