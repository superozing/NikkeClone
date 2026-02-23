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
    private readonly Vector3 _punch;
    private readonly float _duration;
    private readonly int _vibrato;
    private readonly float _elasticity;

    /// <summary>
    /// PunchScaleUIAnimation 생성자
    /// </summary>
    /// <param name="target">크기를 부풀릴 대상</param>
    /// <param name="punch">부풀어오르는 정도 (Vector3)</param>
    /// <param name="duration">다시 원래 크기로 돌아오기까지 걸리는 시간</param>
    /// <param name="vibrato">반동 시 떨림 횟수</param>
    /// <param name="elasticity">0~1 사이 탄성. 1에 가까울수록 강한 반발력</param>
    public PunchScaleUIAnimation(RectTransform target, Vector3 punch, float duration = 0.15f, int vibrato = 1, float elasticity = 0.5f)
    {
        _target = target;
        _punch = punch;
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

        // 이전 트윈 취소 후 초기화 (연타 방어)
        _target.DOKill();
        _target.localScale = Vector3.one;

        var sequence = DOTween.Sequence();

        if (delay > 0f)
        {
            sequence.AppendInterval(delay);
        }

        sequence.Append(_target.DOPunchScale(_punch, _duration, _vibrato, _elasticity));
        sequence.OnComplete(() =>
        {
            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }
}
