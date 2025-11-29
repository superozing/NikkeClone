using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class InventoryOpenAnimation : IUIAnimation
{
    private readonly RectTransform _bgTransform;
    private readonly float _duration;
    private readonly float _moveOffset;

    public InventoryOpenAnimation(RectTransform bgTransform, float duration = 0.3f, float moveOffset = 100f)
    {
        _bgTransform = bgTransform;
        _duration = duration;
        _moveOffset = moveOffset;
    }

    public async Task ExecuteAsync(CanvasGroup cg)
    {
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);

        // 1. 배경 연출 (0.0s ~ duration)
        if (_bgTransform != null)
        {
            _bgTransform.localScale = new Vector3(0f, 1f, 1f);

            seq.Append(_bgTransform.DOScaleX(1f, _duration).SetEase(Ease.OutQuart));
        }

        // 2. 콘텐츠 연출 (배경 완료 후 시작)
        if (cg != null)
        {
            RectTransform contentRT = cg.GetComponent<RectTransform>();
            Vector2 targetPos = contentRT.anchoredPosition;

            // 시작 위치: 아래로 내림
            contentRT.anchoredPosition = targetPos + new Vector2(0, -_moveOffset);

            cg.alpha = 0f;
            cg.interactable = false;

            seq.AppendInterval(0.2f);

            seq.Append(cg.DOFade(1f, _duration).SetEase(Ease.OutQuad));
            seq.Join(contentRT.DOAnchorPos(targetPos, _duration).SetEase(Ease.OutQuart));
        }

        await seq.Play().AsyncWaitForCompletion();

        if (cg != null)
            cg.interactable = true;
    }
}