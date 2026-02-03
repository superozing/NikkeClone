using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 모든 UI 연출 클래스가 구현해야 할 인터페이스입니다.
/// </summary>
public interface IUIAnimation
{
    /// <summary>
    /// 연출을 비동기적으로 실행합니다.
    /// 대상(Context)과 설정은 생성자에서 주입받습니다.
    /// </summary>
    /// <param name="delay">실행 전 대기 시간(초)</param>
    /// <returns>연출 완료를 알리는 Task</returns>
    Task ExecuteAsync(float delay = 0f);
}