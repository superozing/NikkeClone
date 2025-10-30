using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 모든 UI 연출 클래스가 구현해야 할 인터페이스입니다.
/// </summary>
public interface IUIAnimation
{
    /// <summary>
    /// 대상 GameObject에 대해 연출을 비동기적으로 실행합니다.
    /// </summary>
    /// <param name="target">연출을 적용할 대상 GameObject</param>
    /// <returns>연출 완료를 알리는 Task</returns>
    Task ExecuteAsync(GameObject target);
}