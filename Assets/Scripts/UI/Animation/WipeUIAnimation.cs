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
    /// <param name="material">조작할 Material 인스턴스</param>
    /// <param name="startValue">시작 Cutoff 값</param>
    /// <param name="endValue">목표 Cutoff 값</param>
    /// <param name="duration">진행 시간</param>
    /// <param name="ease">Ease Function</param>
    public WipeUIAnimation(Material material, float startValue, float endValue, float duration = 0.5f, Ease ease = Ease.InOutQuad)
    {
        _targetMaterial = material;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _ease = ease;
        _propertyId = Shader.PropertyToID("_CutOff");
    }

    public async Task ExecuteAsync(CanvasGroup cg, float delay = 0f)
    {
        // CanvasGroup이 있다면 잠시 입력을 차단하거나 상호작용을 비활성화합니다.
        if (cg != null)
            cg.interactable = false;

        if (_targetMaterial == null)
            return;

        // 1. 시작 값 설정 (즉시 반영)
        _targetMaterial.SetFloat(_propertyId, _startValue);

        // 2. 딜레이 대기
        if (delay > 0f)
            await Task.Delay((int)(delay * 1000));

        // 3. 트윈 실행
        await _targetMaterial.DOFloat(_endValue, _propertyId, _duration)
            .SetEase(_ease)
            .SetUpdate(true) // TimeScale 무시 (로딩 중 멈춤 방지)
            .AsyncWaitForCompletion();

        // 끝난 후 CanvasGroup 인터랙션을 복구할지 말지는 호출자(또는 뷰)에서 처리하거나
        // 필요하다면 cg.interactable = true; 를 호출할 수 있습니다.
    }
}