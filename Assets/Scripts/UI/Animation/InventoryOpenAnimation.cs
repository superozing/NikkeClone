using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class InventoryOpenAnimation : IUIAnimation
{
    private readonly RectTransform _bgTransform;
    private readonly float _duration;

    /// <summary>
    /// 인벤토리 탭 전용 등장 연출입니다.
    /// 배경(RectTransform)이 먼저 펼쳐지고, 그 다음 컨텐츠(CanvasGroup)가 나타납니다.
    /// </summary>
    /// <param name="bgTransform">X축으로 펼쳐질 배경 트랜스폼</param>
    /// <param name="duration">각 단계별 연출 시간 (기본 0.3초)</param>
    public InventoryOpenAnimation(RectTransform bgTransform, float duration = 0.3f)
    {
        _bgTransform = bgTransform;
        _duration = duration;
    }

    public async Task ExecuteAsync(CanvasGroup cg)
    {
        // 시퀀스 생성
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true); // timeScale 무시 여부 (필요 시 조정)

        // 1. 초기 상태 설정
        if (_bgTransform != null)
        {
            _bgTransform.localScale = new Vector3(0f, 1f, 1f);

            // 단계 1: 배경이 X축으로 펼쳐짐
            seq.Append(_bgTransform.DOScaleX(1f, _duration).SetEase(Ease.OutBack));
        }

        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;

            // 단계 2: 배경이 다 펼쳐진 후 내용물 페이드 인 (배경 연출 시간의 2/3 정도 빠르게)
            seq.Append(cg.DOFade(1f, _duration * 0.6f).SetEase(Ease.OutQuad));
        }

        // 3. 실행 및 대기
        await seq.Play().AsyncWaitForCompletion();

        // 4. 완료 후 상호작용 활성화
        if (cg != null)
            cg.interactable = true;
    }
}