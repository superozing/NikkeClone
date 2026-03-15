using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.TestScene;
    public string DefaultActionMapKey => "None";
    public List<string> RequiredDataFiles => new()
    {
        "NikkeGameData.json",
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() 합니다.");
    }

    async Task IScene.InitAsync()
    {
        Debug.Log("Test Scene InitAsync() 합니다.");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        ShowTestUI();
        await Task.CompletedTask;
    }

    /// <summary>
    /// UI_TabGroupPopup 테스트를 위한 메서드
    /// </summary>
    private async void ShowTestUI()
    {
        await Task.Delay(1000);
        Debug.Log("[TestScene] UI_TabGroupPopup 테스트 시작");

        var tabGroupVM = new TabGroupPopupViewModel();
        await Managers.UI.ShowAsync<UI_TabGroupPopup>(tabGroupVM);

        Debug.Log("[TestScene] UI_TabGroupPopup 테스트 완료");
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() 합니다.");
    }
}