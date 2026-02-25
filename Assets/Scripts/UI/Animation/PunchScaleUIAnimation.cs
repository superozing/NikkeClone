using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 대상 RectTransform의 크기를 순간적으로 부풀렸다 줄이는(Punch) 연출입니다.
/// 총기 반동(조준선 팽창), 피격 반짝임, 버튼 클릭 피드백 등에 사용됩니다.
/// </summary>
public class PunchScaleUIAnimation : IUIAnimation
{
    private readonly RectTransform _target;
    private readonly Vector3 _punchScale;
    private readonly float _duration;
    private readonly int _vibrato;
    private readonly float _elasticity;

    /// <summary>
    /// PunchScaleUIAnimation 생성자
    /// </summary>
    /// <param name="target">크기를 부풀릴 대상</param>
    /// <param name="punchScale">도달할 최종 스케일 (예: Vector3.one * 1.3f)</param>
    /// <param name="duration">다시 원래 크기로 돌아오기까지 걸리는 시간</param>
    /// <param name="vibrato">반동 시 떨림 횟수</param>
    /// <param name="elasticity">0~1 사이 탄성. 1에 가까울수록 강한 반발력</param>
    public PunchScaleUIAnimation(RectTransform target, Vector3 punchScale, float duration = 0.15f, int vibrato = 1, float elasticity = 0.5f)
    {
        _target = target;
        _punchScale = punchScale;
        _duration = duration;
        _vibrato = vibrato;
        _elasticity = elasticity;
    }

    public Task ExecuteAsync(float delay = 0f)
    {
        if (_target == null)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource<bool>();

        // 이전 트윈 취소 
        _target.DOKill();

        // 무조건 기본 스케일(1,1,1)에서 시작
        _target.localScale = Vector3.one;

        var sequence = DOTween.Sequence();

        if (delay > 0f)
        {
            sequence.AppendInterval(delay);
        }

        // 1. 외부에서 주입한 절대 목표 스케일(_punchScale)까지 빠르게 커졌다가
        // 2. 다시 원래 크기로 돌아오는 명시적인 DOScale 시퀀스를 만듭니다.
        sequence.Append(_target.DOScale(_punchScale, _duration * 0.3f).SetEase(Ease.OutElastic, _vibrato, _elasticity));
        sequence.Append(_target.DOScale(Vector3.one, _duration * 0.7f).SetEase(Ease.OutSine));

        sequence.OnComplete(() =>
        {
            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }
}
