using UnityEngine;

/// <summary>
/// 니케의 엄폐물입니다.
/// CombatNikke의 자식 오브젝트로 배치됩니다.
/// </summary>
public class NikkeCover : MonoBehaviour
{
    [SerializeField] private float _damageReduction = 0.5f;  // 엄폐 시 데미지 감소율

    /// <summary>엄폐 시 데미지 감소율 (0~1)</summary>
    public float DamageReduction => _damageReduction;

    // TODO Phase 3: 엄폐물 파괴 로직 (선택적)
}
