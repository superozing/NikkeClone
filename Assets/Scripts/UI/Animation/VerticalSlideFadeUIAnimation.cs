using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 아래에서 위로 올라오며 페이드 인 되는 등장 연출입니다.
/// </summary>
public class VerticalSlideFadeUIAnimation : IUIAnimation
{
    private readonly float _duration;
    private readonly float _offsetY;
    private readonly Ease _ease;

    public VerticalSlideFadeUIAnimation(float duration = 0.3f, float offsetY = 100f, Ease ease = Ease.OutQuart)
    {
        _duration = duration;
        _offsetY = offsetY;
        _ease = ease;
    }

    public async Task ExecuteAsync(CanvasGroup cg)
    {
        if (cg == null) return;

        RectTransform rt = cg.GetComponent<RectTransform>();
        if (rt == null) return;

        // 호출 시점의 위치를 최종 목적지로 가정합니다.
        // 따라서 View에서 호출 전에 위치를 초기화(Reset) 해두어야 합니다.
        Vector2 targetPos = rt.anchoredPosition;

        // 1. 초기 상태 설정 (아래로 내리고 투명하게)
        rt.anchoredPosition = targetPos + new Vector2(0, -_offsetY);
        cg.alpha = 0f;
        cg.interactable = false;

        // 2. 시퀀스 구성 (페이드 + 이동)
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        seq.Append(cg.DOFade(1f, _duration).SetEase(Ease.OutQuad));
        seq.Join(rt.DOAnchorPos(targetPos, _duration).SetEase(_ease));

        // 3. 실행 및 대기
        await seq.Play().AsyncWaitForCompletion();

        cg.interactable = true;
    }
}