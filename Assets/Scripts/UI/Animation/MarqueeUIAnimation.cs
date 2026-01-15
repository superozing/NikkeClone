using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 텍스트가 마스크 영역보다 길 경우 좌우로 흐르는(Yoyo) 연출을 수행합니다.
/// </summary>
public class MarqueeUIAnimation : IUIAnimation
{
    private readonly RectTransform _maskRect;
    private readonly float _speed;
    private readonly float _delay;
    private Tween _currentTween;

    public MarqueeUIAnimation(RectTransform maskRect, float speed = 30f, float defaultDelay = 1.5f)
    {
        _maskRect = maskRect;
        _speed = speed;
        _delay = defaultDelay;
    }

    public Task ExecuteAsync(CanvasGroup cg, float delay = 0f)
    {
        if (cg == null || _maskRect == null)
            return Task.CompletedTask;

        RectTransform targetRect = cg.GetComponent<RectTransform>();
        if (targetRect == null)
            return Task.CompletedTask;

        // 기존 애니메이션 정리 및 위치 초기화
        Kill();

        // 1. 초기 위치 설정 (즉시 적용)
        targetRect.anchoredPosition = Vector2.zero;

        // 텍스트(내용물)가 마스크(창문)보다 넓으면 스크롤 연출 시작
        if (targetRect.rect.width > _maskRect.rect.width)
        {
            // 이동 거리 계산 (텍스트 끝이 마스크 끝에 닿을 정도 + 여유분 20px)
            float moveDistance = targetRect.rect.width - _maskRect.rect.width + 20f;
            float duration = moveDistance / _speed;

            // 2. 시퀀스 구성
            Sequence seq = DOTween.Sequence();
            seq.SetUpdate(true);
            seq.SetLink(targetRect.gameObject); // 오브젝트 파괴 시 트윈 자동 제거

            // 3. 딜레이 적용 (기본 대기 시간 + 추가 딜레이)
            float totalDelay = _delay + delay;
            if (totalDelay > 0f)
                seq.AppendInterval(totalDelay);

            // 4. 딜레이가 끝난 직후, 위치를 (0,0)으로 강제 재설정
            // 대기 시간 동안 LayoutGroup이 위치를 변경했을 가능성을 차단합니다.
            seq.AppendCallback(() =>
            {
                targetRect.anchoredPosition = Vector2.zero;
            });

            // 5. 무한 루프 애니메이션 연결
            seq.Append(targetRect.DOAnchorPosX(-moveDistance, duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo));

            _currentTween = seq;
        }

        // 무한 루프 애니메이션이므로 완료를 기다리지 않고 즉시 반환
        return Task.CompletedTask;
    }

    /// <summary>
    /// 실행 중인 트윈을 강제로 종료합니다.
    /// </summary>
    public void Kill()
    {
        _currentTween?.Kill();
        _currentTween = null;
    }
}