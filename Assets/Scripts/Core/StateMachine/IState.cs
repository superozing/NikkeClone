/// <summary>
/// 범용 상태 인터페이스입니다.
/// 특정 타입 T를 소유자(Owner)로 받아 상태 진입, 실행, 종료 로직을 정의합니다.
/// </summary>
/// <typeparam name="T">상태를 소유하는 객체의 타입</typeparam>
public interface IState<T>
{
    /// <summary>
    /// 상태에 진입할 때 호출됩니다.
    /// </summary>
    /// <param name="owner">상태를 소유하는 객체</param>
    void Enter(T owner);

    /// <summary>
    /// 상태가 활성화된 동안 매 프레임 호출됩니다.
    /// </summary>
    /// <param name="owner">상태를 소유하는 객체</param>
    void Execute(T owner);

    /// <summary>
    /// 상태에서 이탈할 때 호출됩니다.
    /// </summary>
    /// <param name="owner">상태를 소유하는 객체</param>
    void Exit(T owner);
}
