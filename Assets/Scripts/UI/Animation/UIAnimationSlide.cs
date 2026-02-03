using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class UIAnimationSlide : IUIAnimation
{
    private readonly RectTransform _target;
    private readonly Vector2 _from;
    private readonly Vector2 _to;
    private readonly float _duration;
    private readonly Ease _ease;

    public UIAnimationSlide(RectTransform target, Vector2 from, Vector2 to, float duration = 0.3f, Ease ease = Ease.OutQuart)
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

        // 1. 초기화
        _target.anchoredPosition = _from;

        // 2. 딜레이
        if (delay > 0)
            await Task.Delay(System.TimeSpan.FromSeconds(delay));

        // 3. 실행
        await _target.DOAnchorPos(_to, _duration)
            .SetEase(_ease)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
}
