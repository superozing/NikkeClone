using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

/// <summary>
/// 풀버스트 진입 시 화면 중앙에 나타나는 알림 팝업입니다.
/// </summary>
public class UI_FullBurstPopup : UI_Popup
{
    [SerializeField] private RectTransform _contentRoot;
    [SerializeField] private float _displayDuration = 0.2f;

    private IUIAnimation _showAnim;
    private IUIAnimation _hideAnim;

    protected override void Awake()
    {
        base.Awake();

        if (_contentRoot != null && _canvasGroup != null)
        {
            // 등장 연출: Scale (OutBack) + Fade In
            var scaleIn = new ScaleUIAnimation(_contentRoot, Vector3.zero, Vector3.one, 0.3f);
            var fadeIn = new FadeUIAnimation(_canvasGroup, 0f, 1f, 0.2f);
            _showAnim = new UIAnimationComposite(scaleIn, fadeIn);

            // 퇴장 연출: Scale Up (1 -> 1.5) + Fade Out
            var scaleUp = new ScaleUIAnimation(_contentRoot, Vector3.one, Vector3.one * 1.5f, 0.3f, Ease.InQuad);
            var fadeOut = new FadeUIAnimation(_canvasGroup, 1f, 0f, 0.2f);
            _hideAnim = new UIAnimationComposite(scaleUp, fadeOut);
        }
    }

    private void OnEnable()
    {
        PlaySequence();
    }

    private async void PlaySequence()
    {
        if (_showAnim == null || _hideAnim == null) return;

        // 1. 게임 일시 정지
        Managers.Time.PauseGame();

        // 2. 등장
        await _showAnim.ExecuteAsync();

        // 3. 대기
        await Task.Delay((int)(_displayDuration * 1000));

        // 4. 퇴장
        await _hideAnim.ExecuteAsync();

        // 5. 게임 재개
        Managers.Time.ResumeGame();

        // 6. 삭제
        Managers.UI.Close(this);
    }
}
