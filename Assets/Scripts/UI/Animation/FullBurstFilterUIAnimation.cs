using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 풀버스트 필터(비네팅 + 색상 보간) 전용 UI 애니메이션 클래스입니다.
/// </summary>
public class FullBurstFilterUIAnimation : IUIAnimation
{
    public enum FilterState { TurnOn, TurnOff }

    private readonly Material _filterMaterial;
    private readonly FilterState _state;
    private readonly float _fadeDuration;
    private readonly float _pingPongDuration;

    // 셰이더 프로퍼티 ID 캐싱
    private static readonly int PropAlpha = Shader.PropertyToID("_Alpha");
    private static readonly int PropLerpT = Shader.PropertyToID("_LerpT");

    public FullBurstFilterUIAnimation(Material filterMat, FilterState state, float fadeDur = 0.5f, float pingPongDur = 1.0f)
    {
        _filterMaterial = filterMat;
        _state = state;
        _fadeDuration = fadeDur;
        _pingPongDuration = pingPongDur;
    }

    public async Task ExecuteAsync(float delay = 0f)
    {
        if (_filterMaterial == null) return;

        if (_state == FilterState.TurnOn)
        {
            // 1. Alpha Fade In
            _filterMaterial.DOKill();
            var fadeTween = _filterMaterial.DOFloat(1f, PropAlpha, _fadeDuration)
                .SetDelay(delay)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);

            // 2. Color Ping-Pong (무한 루프)
            _filterMaterial.SetFloat(PropLerpT, 0f);
            _filterMaterial.DOFloat(1f, PropLerpT, _pingPongDuration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLoops(-1, LoopType.Yoyo);

            await fadeTween.AsyncWaitForCompletion();
        }
        else
        {
            // 1. 모든 관련 트윈 정지
            _filterMaterial.DOKill();

            // 2. Alpha Fade Out
            await _filterMaterial.DOFloat(0f, PropAlpha, _fadeDuration)
                .SetDelay(delay)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .AsyncWaitForCompletion();
        }
    }
}
