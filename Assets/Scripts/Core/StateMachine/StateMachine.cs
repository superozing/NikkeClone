/// <summary>
/// 범용 상태 머신 클래스입니다.
/// 특정 타입 T를 소유자로 갖고, IState 구현체들 간의 상태 전환을 관리합니다.
/// </summary>
/// <typeparam name="T">상태 머신을 소유하는 객체의 타입</typeparam>
public class StateMachine<T>
{
    private readonly T _owner;
    private IState<T> _currentState;

    /// <summary>
    /// 현재 활성화된 상태를 반환합니다.
    /// </summary>
    public IState<T> CurrentState => _currentState;

    /// <summary>
    /// 상태 머신을 초기화합니다.
    /// </summary>
    /// <param name="owner">상태 머신을 소유하는 객체</param>
    public StateMachine(T owner) => _owner = owner;

    /// <summary>
    /// 현재 상태를 새로운 상태로 전환합니다.
    /// 기존 상태의 Exit()를 호출한 후, 새 상태의 Enter()를 호출합니다.
    /// </summary>
    /// <param name="newState">전환할 새 상태</param>
    public void ChangeState(IState<T> newState)
    {
        _currentState?.Exit(_owner);
        _currentState = newState;
        _currentState?.Enter(_owner);
    }

    /// <summary>
    /// 현재 상태의 Execute 메서드를 호출합니다.
    /// 소유자의 Update() 등에서 매 프레임 호출해야 합니다.
    /// </summary>
    public void Update() => _currentState?.Execute(_owner);
}
