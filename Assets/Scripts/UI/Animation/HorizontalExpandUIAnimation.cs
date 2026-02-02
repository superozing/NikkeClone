using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 대상을 가로(X축)로 펼치는 등장 연출입니다.
/// </summary>
public class HorizontalExpandUIAnimation : IUIAnimation
{
    private readonly float _duration;
    private readonly Ease _ease;

    private readonly CanvasGroup _cg;

    public HorizontalExpandUIAnimation(CanvasGroup cg, float duration = 0.3f, Ease ease = Ease.OutQuart)
    {
        _cg = cg;
        _duration = duration;
        _ease = ease;
    }

    public async Task ExecuteAsync(float delay = 0f)
    {
        if (_cg == null) return;

        // 트랜스폼 제어를 위해 RectTransform 접근
        RectTransform rt = _cg.GetComponent<RectTransform>();
        if (rt == null) return;

        // 1. 초기 상태: X축 스케일 0
        rt.localScale = new Vector3(0f, 1f, 1f);

        // 2. 딜레이 대기
        if (delay > 0)
            await Task.Delay(System.TimeSpan.FromSeconds(delay));

        // 3. 연출 실행
        // 외부 delay는 View에서 처리됨 -> 이제 내부에서 처리됨
        await rt.DOScaleX(1f, _duration)
                .SetEase(_ease)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
    }
}