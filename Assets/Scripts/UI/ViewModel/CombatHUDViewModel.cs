using UI;

/// <summary>
/// 전투 HUD 뷰모델입니다.
/// </summary>
public class CombatHUDViewModel : ViewModelBase
{
    public CombatNikke[] Nikkes { get; }
    
    public CombatHUDViewModel(CombatNikke[] nikkes)
    {
        Nikkes = nikkes;
    }
    
    // TODO Phase 4: 진행률
    // TODO Phase 5: 남은 시간
    // TODO Phase 8: 버스트 게이지
}
