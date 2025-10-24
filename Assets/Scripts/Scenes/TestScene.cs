using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;
    public List<string> RequiredDataFiles => new() 
    { 
        "NikkeGameData.json", 
        "ItemGameData.json"
    };


    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() 합니다.");
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() 합니다.");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        _ = Managers.UI.ShowAsync<UI_PopupTest>(new PopupTestViewModel());
        _ = Managers.UI.ShowAsync<UI_Money>(new MoneyViewModel());
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() 합니다.");
    }
}