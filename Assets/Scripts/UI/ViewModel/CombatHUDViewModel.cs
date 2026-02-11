using UI;

/// <summary>
/// 전투 HUD 뷰모델입니다.
/// </summary>
public class CombatHUDViewModel : ViewModelBase
{
    public CombatNikke[] Nikkes { get; }

    // Phase 5: 현재 활성 니케 인덱스 (데이터 바인딩용, 필요시 ReactiveProperty로 변경 가능)
    public int ActiveNikkeIndex { get; set; }

    public CombatHUDViewModel(CombatNikke[] nikkes)
    {
        Nikkes = nikkes;
    }
    
    // TODO Phase 4: 진행률
    // TODO Phase 5: 남은 시간
    // TODO Phase 8: 버스트 게이지
}
