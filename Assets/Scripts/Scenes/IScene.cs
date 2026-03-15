using System.Collections.Generic;
using System.Threading.Tasks;

public interface IScene
{
    List<string> RequiredDataFiles { get; }

    public Task InitAsync();
    public void Clear();
    public eSceneType SceneType { get; }
    public string DefaultActionMapKey { get; }
}
