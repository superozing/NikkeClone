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
    /// 최대 차지 배율 (기본 1.0, 차지형 무기에서 재정의)
    /// </summary>
    float FullChargeMultiplier { get; }

    /// <summary>
    /// 무기의 적정 사거리
    /// </summary>
    eRangeZone PreferredZone { get; }

    /// <summary>
    /// 현재 조준 중인 타겟이 적정 사거리에 있는지 여부를 나타내는 반응형 속성입니다.
    /// UI(조준선) 등에서 구독하여 피드백을 표시하는 데 사용됩니다.
    /// </summary>
    ReactiveProperty<bool> IsInPreferredZone { get; }

    /// <summary>
    /// 무기를 소유한 니케의 현재 전투 모드 (수동/자동 등)
    /// </summary>
    ReactiveProperty<NikkeClone.Utils.eNikkeCombatMode> CombatMode { get; }

    /// <summary>
    /// 자동 조준 대상의 Screen Space 좌표
    /// </summary>
    ReactiveProperty<Vector2> AutoTargetScreenPosition { get; }

    /// <summary>
    /// 조준선의 현재 화면 픽셀 좌표 (수동/자동 모두 보간/추적된 최종 좌표)
    /// </summary>
    ReactiveProperty<Vector2> CurrentAimScreenPosition { get; }

    /// <summary>
    /// 타겟 거리에 따른 데미지 어드밴티지 배율 반환
    /// </summary>
    float GetRangeAdvantageMultiplier(eRangeZone targetZone);

    /// <summary>
    /// 타겟이 적정 사거리에 있는지 여부
    /// </summary>
    bool IsPreferredZone(eRangeZone targetZone);

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
    /// 매 프레임 무기 로직 실행 (예: 연사, 차징, 지향 보정)
    /// Caller: NikkeAttackSubState.Execute()
    /// </summary>
    /// <param name="owner">무기 소유자</param>
    /// <param name="targetWorldPos">타겟 월드 3D 좌표</param>
    void Update(CombatNikke owner, Vector3 targetWorldPos);

    /// <summary>
    /// 무기 사용 종료 (예: 공격 버튼 뗌, 재장전, 탄약 소진)
    /// Caller: NikkeAttackSubState.Exit()
    /// </summary>
    void Exit(CombatNikke owner, bool isCancel = false);

    /// <summary>
    /// 재장전 (탄약 보충)
    /// Caller: CombatNikke.OnReloadComplete()
    /// </summary>
    void Reload();

    /// <summary>
    /// 탄약 소비
    /// </summary>
    void ConsumeAmmo(int amount);

    /// <summary>
    /// 매 프레임 무기 고유의 로직(차지 게이지 감소, 예열 등) 실행
    /// </summary>
    void Tick(float deltaTime);
}
