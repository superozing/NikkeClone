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
        targetRect.anchoredPosition = Vector2.zero;

        // 텍스트(내용물)가 마스크(창문)보다 넓으면 스크롤 연출 시작
        if (targetRect.rect.width > _maskRect.rect.width)
        {
            // 이동 거리 계산 (텍스트 끝이 마스크 끝에 닿을 정도 + 여유분 20px)
            float moveDistance = targetRect.rect.width - _maskRect.rect.width + 20f;

            // 속도 계산
            float duration = moveDistance / _speed;

            // 요요(Yoyo) 스타일 스크롤
            _currentTween = targetRect.DOAnchorPosX(-moveDistance, duration)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Yoyo) // 무한 반복
                .SetDelay(_delay + delay) // 기본 딜레이 + 인자 딜레이
                .SetUpdate(true) // TimeScale 무관하게 동작 원할 시 true
                .SetLink(targetRect.gameObject); // 오브젝트 파괴 시 트윈 자동 제거
        }

        // 무한 루프 애니메이션이므로 완료를 기다리지 않고 즉시 반환하여 다음 로직 진행
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