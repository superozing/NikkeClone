using System.Threading.Tasks;

/// <summary>
/// UI의 등장(Show)과 퇴장(Hide) 행동을 정의하는 인터페이스입니다.
/// 구체적인 연출 방식(IUIAnimation 사용 여부 등)은 구현체에게 위임합니다.
/// </summary>
public interface IUIShowHideAnimation
{
    /// <summary>
    /// 등장 연출을 실행하고 완료될 때까지 대기합니다.
    /// </summary>
    Task PlayShowAnimationAsync();

    /// <summary>
    /// 퇴장 연출을 실행하고 완료될 때까지 대기합니다.
    /// </summary>
    Task PlayHideAnimationAsync();
}
