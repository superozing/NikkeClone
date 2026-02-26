
namespace NikkeClone.Utils
{
    /// <summary>
    /// 버스트 시스템의 현재 진행 단계를 정의합니다.
    /// </summary>
    public enum eBurstStage
    {
        None = 0,       // 게이지 충전 중
        Stage1 = 1,     // 1단계 버스트 대기
        Stage2 = 2,     // 2단계 버스트 대기
        Stage3 = 3,     // 3단계 버스트 대기
        FullBurst = 4   // 풀버스트 진행 중
    }
}
