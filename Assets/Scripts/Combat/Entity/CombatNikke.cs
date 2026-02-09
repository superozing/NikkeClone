using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 전투용 니케 클래스입니다.
/// Phase 2: 상태 머신과 CombatEntity 상속이 적용되었습니다.
/// </summary>
public class CombatNikke : CombatEntity
{
    // ==================== Data ====================

    private SpriteRenderer _spriteRenderer;

    private NikkeGameData _gameData;
    private UserNikkeData _userData;
    private StateMachine<CombatNikke> _stateMachine;
    private int _slotIndex;
    private int _currentAmmo;
    private Dictionary<eNikkeState, IState<CombatNikke>> _states;

    // Sprite 캐싱 (상태별 이미지)
    private Sprite _idleSprite;
    private Sprite _shootSprite;

    // ==================== Properties ====================

    /// <summary>니케 ID</summary>
    public int NikkeId => _gameData?.id ?? -1;

    /// <summary>니케 이름</summary>
    public string NikkeName => _gameData?.name ?? "Unknown";

    /// <summary>현재 상태</summary>
    public eNikkeState CurrentState { get; private set; }

    /// <summary>배치 슬롯 인덱스</summary>
    public int SlotIndex => _slotIndex;

    /// <summary>현재 탄약</summary>
    public int CurrentAmmo => _currentAmmo;

    /// <summary>최대 탄약</summary>
    public int MaxAmmo => _gameData.weapon.maxAmmo;

    /// <summary>재장전 시간</summary>
    public float ReloadTime => _gameData.weapon.reloadTime;

    /// <summary>공격력 (BaseStatus 기준)</summary>
    /// <remarks>Caller: CombatScene.OnRaptureHit()</remarks>
    public long AttackPower => _baseStatus.attack;

    /// <summary>발사 가능 여부</summary>
    /// <remarks>Caller: CombatScene.HandleClick()</remarks>
    public bool CanFire => _currentAmmo > 0 && !IsDead;

    // ==================== Public Methods ====================

    /// <summary>
    /// 니케 데이터를 주입하고 상태 머신을 초기화합니다.
    /// Caller: CombatScene.InitializeNikkes()
    /// </summary>
    public async Task InitializeAsync(NikkeGameData gameData, UserNikkeData userData, int slotIndex)
    {
        _gameData = gameData;
        _userData = userData;
        _slotIndex = slotIndex;

        // 스탯 계산
        CalculateStatus();
        _currentHp = MaxHp;
        _currentAmmo = MaxAmmo;

        // SpriteRenderer 동적 할당 (Sprite 자식 오브젝트)
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 상태별 Sprite 로드 (Addressables)
        _idleSprite = await Managers.Resource.LoadAsync<Sprite>($"Assets/Textures/Nikke/{_gameData.name}_Idle");
        _shootSprite = await Managers.Resource.LoadAsync<Sprite>($"Assets/Textures/Nikke/{_gameData.name}_Shoot");

        // 상태 머신 초기화
        _stateMachine = new StateMachine<CombatNikke>(this);
        _states = new Dictionary<eNikkeState, IState<CombatNikke>>
        {
            { eNikkeState.Cover, new NikkeCoverState() },
            { eNikkeState.Attack, new NikkeAttackState() },
            { eNikkeState.Reload, new NikkeReloadState() },
            { eNikkeState.Stunned, new NikkeStunnedState() },
            { eNikkeState.Dead, new NikkeDeadState() }
        };

        // 초기 상태 진입
        ChangeState(eNikkeState.Cover);

        Debug.Log($"[CombatNikke] Initialized: {NikkeName} (Lv.{_userData.level.Value}) HP:{MaxHp}");
    }

    /// <summary>
    /// 상태를 변경합니다.
    /// Caller: 각 상태 클래스의 Execute()
    /// </summary>
    public void ChangeState(eNikkeState newState)
    {
        if (_states.ContainsKey(newState))
        {
            CurrentState = newState;
            _stateMachine.ChangeState(_states[newState]);
        }
        else
        {
            Debug.LogError($"[CombatNikke] State not found: {newState}");
        }
    }

    /// <summary>
    /// 상태에 따라 스프라이트를 변경합니다.
    /// Caller: NikkeCoverState.Enter(), NikkeAttackState.Enter()
    /// </summary>
    public void SetStateSprite(eNikkeState state)
    {
        if (_spriteRenderer == null) return;

        Sprite newSprite = state switch
        {
            eNikkeState.Cover => _idleSprite,
            eNikkeState.Attack => _shootSprite,
            eNikkeState.Reload => _idleSprite,
            _ => _spriteRenderer.sprite // Dead/Stunned: 기존 유지
        };

        if (newSprite == null) return;

        _spriteRenderer.sprite = newSprite;

        // 500x500 크기로 비율 유지하며 스케일 조정
        AdjustSpriteScale(newSprite, 500f);
    }

    /// <summary>
    /// 스프라이트를 목표 높이에 맞게 비율 유지하며 스케일 조정합니다.
    /// Caller: SetStateSprite()
    /// </summary>
    private void AdjustSpriteScale(Sprite sprite, float targetHeight)
    {
        if (sprite == null || _spriteRenderer == null) return;

        // 스프라이트의 월드 유닛 높이 계산
        float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;

        // 목표 높이를 픽셀 단위에서 월드 유닛으로 변환 (PPU 100 기준)
        float targetWorldHeight = targetHeight / 100f;

        // 세로 크기 기준으로 비율 유지하며 스케일 계산
        float uniformScale = targetWorldHeight / spriteHeight;

        _spriteRenderer.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }

    /// <summary>
    /// 대상 랩쳐를 공격합니다.
    /// Caller: CombatScene.OnRaptureHit()
    /// </summary>
    public void Fire(CombatRapture target)
    {
        ConsumeAmmo();
        target.TakeDamage(AttackPower);
    }

    /// <summary>
    /// 탄약을 소비합니다.
    /// Caller: Fire(), CombatScene.HandleClick() (빗나감)
    /// </summary>
    public void ConsumeAmmo(int amount = 1)
    {
        _currentAmmo = Mathf.Max(0, _currentAmmo - amount);
        Debug.Log($"[{NikkeName}] Ammo: {_currentAmmo}/{MaxAmmo}");

        if (_currentAmmo <= 0)
        {
            ChangeState(eNikkeState.Reload);
        }
    }

    /// <summary>
    /// 공격을 시작합니다.
    /// Caller: CombatScene.HandleInput()
    /// </summary>
    public void StartAttack()
    {
        if (IsDead || CurrentState == eNikkeState.Attack) return;
        ChangeState(eNikkeState.Attack);
    }

    /// <summary>
    /// 공격을 중지합니다.
    /// Caller: CombatScene.HandleInput()
    /// </summary>
    public void StopAttack()
    {
        if (IsDead) return;
        if (CurrentState == eNikkeState.Reload) return; // 이미 재장전 중이면 무시

        // 탄약이 가득 찼으면 바로 Cover, 아니면 Reload
        if (_currentAmmo >= MaxAmmo)
            ChangeState(eNikkeState.Cover);
        else
            ChangeState(eNikkeState.Reload);
    }

    /// <summary>
    /// 탄약을 충전합니다.
    /// Caller: NikkeReloadState.Exit()
    /// </summary>
    public void RefillAmmo()
    {
        _currentAmmo = MaxAmmo;
        Debug.Log($"[{NikkeName}] Ammo Refilled: {CurrentAmmo}/{MaxAmmo}");
    }

    // ==================== Private Methods ====================

    private void CalculateStatus()
    {
        if (_gameData == null || _userData == null) return;

        // 기본 스탯 복사
        _baseStatus = new StatusData
        {
            hp = _gameData.status.hp,
            attack = _gameData.status.attack,
            defense = _gameData.status.defense
        };

        // 레벨 보정: 1 + (Lv-1) * 0.05
        float levelMultiplier = 1 + (_userData.level.Value - 1) * 0.05f;

        // 스탯 적용
        _baseStatus.hp = (long)(_baseStatus.hp * levelMultiplier);
        _baseStatus.attack = (long)(_baseStatus.attack * levelMultiplier);
        _baseStatus.defense = (long)(_baseStatus.defense * levelMultiplier);
    }

    // ==================== Test Code (Phase 2 Only) ====================

    private void Update()
    {
        _stateMachine?.Update();
    }
}
