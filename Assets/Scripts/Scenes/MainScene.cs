using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.MainScene;
    public string DefaultActionMapKey => "None";
    public List<string> RequiredDataFiles => new List<string>()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Main Scene Awake() 합니다.");
    }

    async Task IScene.InitAsync()
    {
        Debug.Log("Main Scene InitAsync() 합니다.");
        ShowTestUI();
        await Task.CompletedTask;
    }

    /// <summary>
    /// UI_TabGroupPopup 테스트를 위한 메서드
    /// </summary>
    private async void ShowTestUI()
    {
        var tabGroupVM = new TabGroupPopupViewModel();
        await Managers.UI.ShowAsync<UI_TabGroupPopup>(tabGroupVM);
    }

    void IScene.Clear()
    {
        Debug.Log("Main Scene Clear() 합니다.");
    }

}
