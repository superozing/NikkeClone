using System.Threading.Tasks;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// 니케의 시각적 요소(스프라이트, 이펙트)와 카메라 연동을 담당하는 뷰 클래스입니다.
/// </summary>
public class NikkeView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    
    // 캐싱된 스프라이트
    private Sprite _idleSprite;
    private Sprite _shootSprite; // 임시: 공격 모션 스프라이트 (실제론 애니메이션일 수 있음)
    
    // 카메라
    private CinemachineCamera _vcam;
    private string _cameraKey;
    private const int CAM_PRIORITY_ACTIVE = 200;
    private const int CAM_PRIORITY_INACTIVE = 100; // 낮은 우선순위

    // ==================== Public Methods ====================

    /// <summary>
    /// 뷰를 초기화합니다.
    /// Caller: CombatNikke.InitializeAsync()
    /// </summary>
    public async Task InitializeAsync(NikkeGameData gameData, int slotIndex, CinemachineCamera vcam)
    {
        _vcam = vcam;
        _cameraKey = $"CAM_NIKKE_{slotIndex}";

        // 1. 스프라이트 로드 (Addressables or Resource)
        // 현재는 Resource.LoadAsync 사용 가정 (Phase 2 코드 참고)
        string name = gameData.name;
        _idleSprite = await Managers.Resource.LoadAsync<Sprite>($"Assets/Textures/Nikke/{name}_Idle");
        _shootSprite = await Managers.Resource.LoadAsync<Sprite>($"Assets/Textures/Nikke/{name}_Shoot");

        // 초기 스프라이트 설정
        if (_spriteRenderer != null && _idleSprite != null)
        {
            SetSprite(_idleSprite);
        }

        // 2. 카메라 등록
        if (_vcam != null)
        {
            Managers.Camera.RegisterCamera(_cameraKey, _vcam, CAM_PRIORITY_INACTIVE);
        }
    }

    /// <summary>
    /// 상태에 따른 비주얼을 업데이트합니다.
    /// Caller: CombatNikke (State Change)
    /// </summary>
    public void UpdateVisualState(eNikkeState state)
    {
        Sprite targetSprite = state switch
        {
            eNikkeState.Attack => _shootSprite,
            eNikkeState.Reload => _idleSprite, // 재장전 모션이 있다면 교체
            eNikkeState.Cover => _idleSprite,
            eNikkeState.Stunned => _idleSprite, // 스턴 모션이 있다면 교체
            _ => _idleSprite
        };

        if (targetSprite != null)
        {
            SetSprite(targetSprite);
        }
    }

    /// <summary>
    /// 카메라를 활성화/비활성화합니다.
    /// Caller: CombatNikke.OnActivated/OnDeactivated
    /// </summary>
    public void SetCameraActive(bool isActive)
    {
        if (isActive)
        {
            Managers.Camera.Activate(_cameraKey, 0.2f);
        }
        else
        {
            Managers.Camera.Deactivate(_cameraKey);
        }
    }

    /// <summary>
    /// 사망 이펙트를 재생합니다.
    /// Caller: CombatNikke.Die()
    /// </summary>
    public void PlayDeathEffect()
    {
        // TODO: 사망 파티클 재생 or 흑백 처리
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = Color.gray;
        }
    }

    public void DestroyView()
    {
        // 카메라 등록 해제
        if (Managers.Inst != null && Managers.Camera != null)
        {
            Managers.Camera.UnregisterCamera(_cameraKey);
        }
    }

    // ==================== Private Methods ====================

    private void SetSprite(Sprite sprite)
    {
        if (_spriteRenderer == null || sprite == null) return;

        _spriteRenderer.sprite = sprite;
        AdjustSpriteScale(sprite, 500f); // 500유닛 높이 기준 (Phase 2 로직 유지)
    }

    private void AdjustSpriteScale(Sprite sprite, float targetHeight)
    {
        float spriteHeight = sprite.rect.height / sprite.pixelsPerUnit;
        float targetWorldHeight = targetHeight / 100f;
        float uniformScale = targetWorldHeight / spriteHeight;

        _spriteRenderer.transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
    }
}
