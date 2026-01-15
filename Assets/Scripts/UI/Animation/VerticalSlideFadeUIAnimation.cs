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

    public async Task ExecuteAsync(CanvasGroup cg, float delay = 0f)
    {
        if (cg == null) return;

        RectTransform rt = cg.GetComponent<RectTransform>();
        if (rt == null) return;

        // 호출 시점의 위치를 최종 목적지로 가정합니다.
        Vector2 targetPos = rt.anchoredPosition;

        // 1. 초기 상태 설정 (투명하게만 설정, 위치는 건드리지 않음)
        // 중요: 위치를 미리 변경하면 대기 시간(Delay) 동안 LayoutGroup이 다시 원위치 시킬 수 있습니다.
        cg.alpha = 0f;
        cg.interactable = false;

        // 2. 시퀀스 구성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // 3. 딜레이 적용 (SetDelay 대신 Interval 사용)
        // Interval은 시퀀스의 일부로 동작하므로 순차 실행을 보장합니다.
        seq.AppendInterval(delay);

        // 4. 딜레이가 끝난 직후, 애니메이션 시작 바로 전에 위치를 오프셋 위치로 강제 이동합니다.
        // 이렇게 하면 LayoutGroup이 덮어쓴 위치를 다시 보정하여 연출이 정상적으로 보이게 됩니다.
        seq.AppendCallback(() =>
        {
            rt.anchoredPosition = targetPos + new Vector2(0, -_offsetY);
        });

        // 5. 애니메이션 정의 (Fade In + Move To Target)
        seq.Append(cg.DOFade(1f, _duration).SetEase(Ease.OutQuad));
        seq.Join(rt.DOAnchorPos(targetPos, _duration).SetEase(_ease));

        // 6. 실행 및 대기
        await seq.Play().AsyncWaitForCompletion();

        cg.interactable = true;
    }
}