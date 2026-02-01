using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 아래에서 위로 올라오며 페이드 인 되는 등장 연출입니다.
/// </summary>
public class VerticalSlideFadeUIAnimation : IUIAnimation
{
    private readonly CanvasGroup _cg;
    private readonly float _duration;
    private readonly float _offsetY;
    private readonly Ease _ease;

    public VerticalSlideFadeUIAnimation(CanvasGroup cg, float duration = 0.3f, float offsetY = 100f, Ease ease = Ease.OutQuart)
    {
        _cg = cg;
        _duration = duration;
        _offsetY = offsetY;
        _ease = ease;
    }

    public async Task ExecuteAsync()
    {
        if (_cg == null) return;

        RectTransform rt = _cg.GetComponent<RectTransform>();
        if (rt == null) return;

        // 호출 시점의 위치를 최종 목적지로 가정합니다.
        Vector2 targetPos = rt.anchoredPosition;

        // 1. 초기 상태 설정
        _cg.alpha = 0f;
        _cg.interactable = false;

        // 2. 시퀀스 구성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // 3. 애니메이션 시작 전 위치 오프셋 적용
        seq.AppendCallback(() =>
        {
            rt.anchoredPosition = targetPos + new Vector2(0, -_offsetY);
        });

        // 4. 애니메이션 정의 (Fade In + Move To Target)
        seq.Append(_cg.DOFade(1f, _duration).SetEase(Ease.OutQuad));
        seq.Join(rt.DOAnchorPos(targetPos, _duration).SetEase(_ease));

        // 5. 실행 및 대기
        await seq.Play().AsyncWaitForCompletion();

        _cg.interactable = true;
    }
}