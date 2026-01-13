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
        Debug.Log("Test Scene Awake() 합니다.");
    }

    void IScene.Init()
    {
        Debug.Log("Test Scene Init() 합니다.");
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");

        ShowTestUI();
    }
    
    /// <summary>
    /// UI_LoadingPopup 테스트를 위한 메서드.
    /// 2초 대기 Task를 실행하여 WipeIn/WipeOut 애니메이션을 확인.
    /// </summary>
    private async void ShowTestUI()
    {
        Debug.Log("[TestScene] UI_LoadingPopup 테스트 시작");
        
        var loadingVM = new LoadingPopupViewModel(async () =>
        {
            Debug.Log("[TestScene] 로딩 작업 시작 (2초 대기)");
            await Task.Delay(2000);
            Debug.Log("[TestScene] 로딩 작업 완료");
        });
        
        await Managers.UI.ShowAsync<UI_LoadingPopup>(loadingVM);
        Debug.Log("[TestScene] UI_LoadingPopup 테스트 완료");
    }

    void IScene.Clear()
    {
        //Debug.Log("Test Scene Clear() 합니다.");
    }
}