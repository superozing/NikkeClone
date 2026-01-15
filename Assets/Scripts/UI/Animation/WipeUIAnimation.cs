using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class WipeUIAnimation : IUIAnimation
{
    private readonly Material _targetMaterial;
    private readonly float _startValue;
    private readonly float _endValue;
    private readonly float _duration;
    private readonly Ease _ease;
    private readonly int _propertyId;

    /// <summary>
    /// WipeUIAnimation 占쏙옙占쏙옙占쏙옙
    /// </summary>
    /// <param name="material">占쏙옙占쏙옙占쏙옙 Material 占싸쏙옙占싹쏙옙</param>
    /// <param name="startValue">占쏙옙占쏙옙 Cutoff 占쏙옙</param>
    /// <param name="endValue">占쏙옙표 Cutoff 占쏙옙</param>
    /// <param name="duration">占쏙옙占쏙옙 占시곤옙</param>
    /// <param name="ease">Ease Function</param>
    public WipeUIAnimation(Material material, float startValue, float endValue, float duration = 0.5f, Ease ease = Ease.InOutQuad)
    {
        _targetMaterial = material;
        _startValue = startValue;
        _endValue = endValue;
        _duration = duration;
        _ease = ease;
        _propertyId = Shader.PropertyToID("_CutOff");
    }

    public async Task ExecuteAsync(CanvasGroup cg, float delay = 0f)
    {
        // CanvasGroup占쏙옙 占쏙옙占싶뤄옙占쏙옙 占쏙옙占쏙옙 占쎈도占쏙옙 占쏙옙占쏙옙構킬占? 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙占쌌니댐옙.
        if (cg != null)
            cg.interactable = false;

        if (_targetMaterial == null)
            return;

        // 1. 占쏙옙占쏙옙 占쏙옙 占쏙옙占쏙옙 (占쏙옙占?占쏙옙占쏙옙)
        _targetMaterial.SetFloat(_propertyId, _startValue);

        // 2. 占쏙옙占쏙옙占쏙옙 占쏙옙占?
        if (delay > 0f)
            await Task.Delay((int)(delay * 1000));

        // 3. 트占쏙옙占쏙옙 占쏙옙占쏙옙
        await _targetMaterial.DOFloat(_endValue, _propertyId, _duration)
            .SetEase(_ease)
            .SetUpdate(true) // TimeScale 占쏙옙占쏙옙 (占싸듸옙 占쏙옙 占쏙옙占쏙옙 占쏙옙占쏙옙)
            .AsyncWaitForCompletion();

        // 占쏙옙占쏙옙占쏙옙 占쏙옙占쏙옙 占쏙옙 占쏙옙占싶뤄옙占쏙옙 占쏙옙占쏙옙占쏙옙 占쏙옙황占쏙옙 占쏙옙占쏙옙 占쌕몌옙占실뤄옙(占쏙옙占쏙옙 占쏙옙占쏙옙) 占쏙옙占썩선 처占쏙옙占쏙옙占쏙옙 占십거놂옙
        // 占십울옙占싹다몌옙 cg.interactable = true; 占쏙옙 호占쏙옙占쏙옙 占쏙옙 占쌍쏙옙占싹댐옙.
    }
}