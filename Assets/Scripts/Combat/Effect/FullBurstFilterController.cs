using UnityEngine;
using System.Collections;

/// <summary>
/// 풀버스트 시 화면 필터(비네팅 + 색상 보간)의 머티리얼 파라미터를 제어하는 컨트롤러입니다.
/// </summary>
public class FullBurstFilterController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Material _filterMaterial;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _pingPongDuration = 1.0f;

    private IUIAnimation _turnOnAnim;
    private IUIAnimation _turnOffAnim;

    private void Awake()
    {
        // [Refactor] 인스펙터에서 직접 주입받은 shared material을 사용합니다.
        // FullScreenPassRendererFeature가 동일한 머티리얼을 참조하므로 즉시 반영됩니다.
        if (_filterMaterial == null)
        {
            Debug.LogWarning("[FullBurstFilterController] Filter Material is missing. Vignette effect will not work.");
            return;
        }

        // 전용 애니메이션 객체 생성
        _turnOnAnim = new FullBurstFilterUIAnimation(_filterMaterial, FullBurstFilterUIAnimation.FilterState.TurnOn, _fadeInDuration, _pingPongDuration);
        _turnOffAnim = new FullBurstFilterUIAnimation(_filterMaterial, FullBurstFilterUIAnimation.FilterState.TurnOff, _fadeOutDuration);

        // 초기 상태 설정
        ResetMaterialParameters();
    }

    /// <summary>
    /// 필터 제어를 위한 BurstSystem 이벤트를 연결합니다.
    /// </summary>
    public void Initialize(CombatBurstSystem burstSystem)
    {
        if (burstSystem == null) return;

        burstSystem.OnFullBurstStarted += Activate;
        burstSystem.OnFullBurstEnded += Deactivate;
    }

    public void Activate()
    {
        _ = _turnOnAnim?.ExecuteAsync();
    }

    public void Deactivate()
    {
        _ = _turnOffAnim?.ExecuteAsync();
    }

    private void OnDestroy()
    {
        // [Note] shared material의 변경사항은 에디터 종료 후에도 남으므로 명시적으로 리셋합니다.
        ResetMaterialParameters();
    }

    private void ResetMaterialParameters()
    {
        if (_filterMaterial == null) return;

        _filterMaterial.SetFloat("_Alpha", 0f);
        _filterMaterial.SetFloat("_LerpT", 0f);
    }
}
