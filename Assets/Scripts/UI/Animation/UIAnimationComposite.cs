using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class UIAnimationComposite : IUIAnimation
{
    private readonly IEnumerable<IUIAnimation> _animations;

    public UIAnimationComposite(params IUIAnimation[] animations)
    {
        _animations = animations;
    }

    public async Task ExecuteAsync()
    {
        if (_animations == null) return;

        var tasks = _animations.Select(a => a.ExecuteAsync());
        await Task.WhenAll(tasks);
    }
}
