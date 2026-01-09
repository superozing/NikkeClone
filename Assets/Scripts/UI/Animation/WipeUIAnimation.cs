using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class WipeUIAnimation : IUIAnimation
{
    private readonly Material _targetMaterial;
    private readonly float _startValue;
    private readonly float _endValue;
    private readonly float _duration;
    private readonly Ease _ease;
    private readonly int _propertyId;

    /// <summary>
    /// WipeUIAnimation 생성자
    /// </summary>
    /// <param name="material">제어할 Material 인스턴스</param>
    /// <param name="startValue">시작 Cutoff 값</param>
    /// <param name="endValue">목표 Cutoff 값</param>
    /// <param name="duration">연출 시간</param>
    /// <param name="ease">Ease Function</param>
    public WipeUIAnimation(Material material, float startValue, float endValue, float duration = 0.5f, Ease ease = Ease.InOutQuad)
    {
        _targetMaterial = material;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _ease = ease;
        _propertyId = Shader.PropertyToID("_Cutoff");
    }

    public async Task ExecuteAsync(CanvasGroup cg, float delay = 0f)
    {
        // CanvasGroup은 인터랙션 차단 용도로 사용하거나, 없으면 무시합니다.
        if (cg != null)
            cg.interactable = false;

        if (_targetMaterial == null)
            return;

        // 1. 시작 값 설정 (즉시 적용)
        _targetMaterial.SetFloat(_propertyId, _startValue);

        // 2. 딜레이 대기
        if (delay > 0f)
            await Task.Delay((int)(delay * 1000));

        // 3. 트위닝 실행
        await _targetMaterial.DOFloat(_endValue, _propertyId, _duration)
            .SetEase(_ease)
            .SetUpdate(true) // TimeScale 무시 (로딩 중 멈춤 방지)
            .AsyncWaitForCompletion();

        // 연출이 끝난 후 인터랙션 복구는 상황에 따라 다르므로(보통 닫힘) 여기선 처리하지 않거나
        // 필요하다면 cg.interactable = true; 를 호출할 수 있습니다.
    }
}