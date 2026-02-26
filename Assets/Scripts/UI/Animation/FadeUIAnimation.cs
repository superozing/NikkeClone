using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FadeUIAnimation : IUIAnimation
{
    private readonly CanvasGroup _target;
    private readonly float _from;
    private readonly float _to;
    private readonly float _duration;
    private readonly Ease _ease;

    public FadeUIAnimation(CanvasGroup target, float from, float to, float duration = 0.3f, Ease ease = Ease.OutQuad)
    {
        _target = target;
        _from = from;
        _to = to;
        _duration = duration;
        _ease = ease;
    }

    public async Task ExecuteAsync(float delay = 0f)
    {
        if (_target == null) return;

        // 1. 초기 상태 설정
        _target.alpha = _from;

        // 2. 실행 (SetDelay 사용)
        await _target.DOFade(_to, _duration)
            .SetDelay(delay)
            .SetEase(_ease)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
}
