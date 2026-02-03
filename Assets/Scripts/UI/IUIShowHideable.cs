using System.Threading.Tasks;

namespace UI
{
    /// <summary>
    /// Show/Hide 연출을 수행할 수 있는 View 객체가 구현해야 할 인터페이스입니다.
    /// 구 IUIShowHideAnimation을 대체합니다.
    /// </summary>
    public interface IUIShowHideable
    {
        /// <summary>
        /// 등장 연출을 실행합니다.
        /// </summary>
        /// <param name="delay">시작 전 지연 시간(초)</param>
        Task PlayShowAnimationAsync(float delay = 0f);

        /// <summary>
        /// 퇴장 연출을 실행합니다.
        /// </summary>
        /// <param name="delay">시작 전 지연 시간(초)</param>
        Task PlayHideAnimationAsync(float delay = 0f);
    }
}
