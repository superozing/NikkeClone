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

    public async Task ExecuteAsync()
    {
        if (_target == null) return;

        _target.anchoredPosition = _from;
        await _target.DOAnchorPos(_to, _duration).SetEase(_ease).AsyncWaitForCompletion();
    }
}
