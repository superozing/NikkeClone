using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;
    public List<string> RequiredDataFiles => new() 
    { 
        "NikkeGameData.json", 
        "ItemGameData.json",
        "MissionGameData.json",
    };

    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() ?⑸땲??");
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() ?⑸땲??");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        ShowTestUI();
    }
    
    /// <summary>
    /// UI_TabGroupPopup ?뚯뒪?몃? ?꾪븳 硫붿꽌??
    /// </summary>
    private async void ShowTestUI()
    {
        await Task.Delay(1000);
        Debug.Log("[TestScene] UI_TabGroupPopup ?뚯뒪???쒖옉");
        
        var tabGroupVM = new TabGroupPopupViewModel();
        await Managers.UI.ShowAsync<UI_TabGroupPopup>(tabGroupVM);

        Debug.Log("[TestScene] UI_TabGroupPopup ?뚯뒪???꾨즺");
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() ?⑸땲??");
    }
}