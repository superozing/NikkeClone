using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 대상 RectTransform의 Scale을 조정하는 UI 애니메이션입니다.
/// </summary>
public class ScaleUIAnimation : IUIAnimation
{
    private readonly RectTransform _target;
    private readonly Vector3 _startScale;
    private readonly Vector3 _targetScale;
    private readonly float _duration;
    private readonly Ease _ease;

    public ScaleUIAnimation(RectTransform target, Vector3 startScale, Vector3 targetScale, float duration = 0.3f, Ease ease = Ease.OutBack)
    {
        _target = target;
        _startScale = startScale;
        _targetScale = targetScale;
        _duration = duration;
        _ease = ease;
    }

    public async Task ExecuteAsync(float delay = 0f)
    {
        if (_target == null) return;

        // 1. 초기 상태 설정
        _target.DOKill();
        _target.localScale = _startScale;

        // 2. 트위닝 실행 (SetDelay 사용) 및 대기
        await _target.DOScale(_targetScale, _duration)
            .SetDelay(delay)
            .SetEase(_ease)
            .SetUpdate(true)
            .AsyncWaitForCompletion();
    }
}
