using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace UI
{
    public class FadeUIAnimation : IUIAnimation
    {
        private readonly CanvasGroup _target;
        private readonly float _from;
        private readonly float _to;
        private readonly float _duration;
        private readonly Ease _ease;

        public FadeUIAnimation(CanvasGroup target, float from, float to, float duration = 0.3f, Ease ease = Ease.OutQuad)
        {
            _target = target;
            _from = from;
            _to = to;
            _duration = duration;
            _ease = ease;
        }

        public async Task ExecuteAsync()
        {
            if (_target == null) return;

            _target.alpha = _from;
            await _target.DOFade(_to, _duration).SetEase(_ease).AsyncWaitForCompletion();
        }
    }
}
