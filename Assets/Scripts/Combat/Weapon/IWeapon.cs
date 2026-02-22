using UnityEngine;

/// <summary>
/// 무기 동작을 정의하는 인터페이스입니다.
/// 사격 로직, 재장전, 탄약 소비 등을 추상화합니다.
/// </summary>
public interface IWeapon
{
    /// <summary>
    /// 무기 타입
    /// </summary>
    eNikkeWeapon WeaponType { get; }

    /// <summary>
    /// 발사 가능 여부 (탄약, 쿨타임 등 체크)
    /// </summary>
    bool CanFire { get; }

    /// <summary>
    /// 현재 탄약 수
    /// // Implements Section 2.1: IWeapon & WeaponBase 리팩토링
    /// </summary>
    ReactiveProperty<int> CurrentAmmo { get; }

    /// <summary>
    /// 최대 탄약 수
    /// </summary>
    int MaxAmmo { get; }

    /// <summary>
    /// 차지 게이지 (0.0 ~ 1.0)
    /// // Implements Section 2.1: IWeapon & WeaponBase 리팩토링
    /// </summary>
    ReactiveProperty<float> ChargeProgress { get; }

    /// <summary>
    /// 재장전 시간 (초)
    /// </summary>
    float ReloadTime { get; }

    /// <summary>
    /// 무기의 공격력 비율 (계수)
    /// </summary>
    float DamagePercent { get; }

    /// <summary>
    /// 무기 사용 시작 (예: 공격 버튼 누름)
    /// Caller: NikkeAttackSubState.Enter()
    /// </summary>
    void Enter(CombatNikke owner);

    /// <summary>
    /// 매 프레임 무기 로직 실행 (예: 연사, 차징, 레이캐스트 판정)
    /// Caller: NikkeAttackSubState.Execute()
    /// </summary>
    /// <param name="owner">무기 소유자</param>
    void Update(CombatNikke owner);

    /// <summary>
    /// 무기 사용 종료 (예: 공격 버튼 뗌, 재장전, 탄약 소진)
    /// Caller: NikkeAttackSubState.Exit()
    /// TestWeapon: 클릭 해제 시 발사 처리
    /// </summary>
    void Exit(CombatNikke owner);

    /// <summary>
    /// 재장전 (탄약 보충)
    /// Caller: CombatNikke.OnReloadComplete()
    /// </summary>
    void Reload();

    /// <summary>
    /// 탄약 소비
    /// </summary>
    void ConsumeAmmo(int amount);
}
