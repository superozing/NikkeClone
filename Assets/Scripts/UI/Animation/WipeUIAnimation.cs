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
    /// WipeUIAnimation ������
    /// </summary>
    /// <param name="material">������ Material �ν��Ͻ�</param>
    /// <param name="startValue">���� Cutoff ��</param>
    /// <param name="endValue">��ǥ Cutoff ��</param>
    /// <param name="duration">���� �ð�</param>
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
        // CanvasGroup�� ���ͷ��� ���� �뵵�� ����ϰų�, ������ �����մϴ�.
        if (cg != null)
            cg.interactable = false;

        if (_targetMaterial == null)
            return;

        // 1. ���� �� ���� (��� ����)
        _targetMaterial.SetFloat(_propertyId, _startValue);

        // 2. ������ ���
        if (delay > 0f)
            await Task.Delay((int)(delay * 1000));

        // 3. Ʈ���� ����
        await _targetMaterial.DOFloat(_endValue, _propertyId, _duration)
            .SetEase(_ease)
            .SetUpdate(true) // TimeScale ���� (�ε� �� ���� ����)
            .AsyncWaitForCompletion();

        // ������ ���� �� ���ͷ��� ������ ��Ȳ�� ���� �ٸ��Ƿ�(���� ����) ���⼱ ó������ �ʰų�
        // �ʿ��ϴٸ� cg.interactable = true; �� ȣ���� �� �ֽ��ϴ�.
    }
}