using UI;

/// <summary>
/// 전투 HUD 뷰모델입니다.
/// </summary>
public class CombatHUDViewModel : ViewModelBase
{
    public CombatNikke[] Nikkes { get; }

    // Phase 5: 현재 활성 니케 인덱스 (데이터 바인딩용, 필요시 ReactiveProperty로 변경 가능)
    public int ActiveNikkeIndex { get; set; }

    // Phase 7.1 Crosshair UI: 현재 조작 중인 니케의 무기 데이터
    public ReactiveProperty<IWeapon> ActiveNikkeWeapon { get; } = new ReactiveProperty<IWeapon>(null);

    public ReactiveProperty<string> TimeText { get; } = new();

    // Phase 8 & 9: 버스트 시스템
    public ReactiveProperty<BurstGaugeViewModel> BurstGauge { get; } = new(null);

    public CombatHUDViewModel(CombatNikke[] nikkes)
    {
        Nikkes = nikkes;
    }
}
