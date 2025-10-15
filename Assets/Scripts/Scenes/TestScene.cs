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
        Debug.Log($"persistentDataPath: {Application.persistentDataPath}");
        Debug.Log("======== 데이터 로드 결과 확인 시작 ========");

        // --- 1. UserData 로드 확인 ---
        if (Managers.Data.UserData.Items != null)
        {
            if (Managers.Data.UserData.Items.TryGetValue(0, out UserItemData jewel))
                Debug.Log($"[UserData] 쥬얼(ID:0) 개수: {jewel.count.Value}");
            if (Managers.Data.UserData.Items.TryGetValue(1, out UserItemData credit))
                Debug.Log($"[UserData] 크레디트(ID:1) 개수: {credit.count.Value}");
        }
        else
        {
            Debug.LogWarning("[UserData] Items 딕셔너리가 null입니다. UserData.json 파일 내용을 확인하세요.");
        }


        // --- 2. GameData (NikkeGameData) 로드 확인 ---
        var nikkeTable = Managers.Data.GetTable<NikkeGameData>();
        if (nikkeTable == null)
        {
            Debug.LogError("[확인 실패] NikkeGameData 테이블이 로드되지 않았습니다.");
        }
        else
        {
            Debug.Log($"[GameData] NikkeGameData.json 로드 성공! 총 {nikkeTable.Count}개의 데이터가 있습니다.");
            StringBuilder sb = new StringBuilder();
            foreach (var nikkeGameData in nikkeTable.Values)
            {
                sb.AppendLine($"  - ID: {nikkeGameData.ID}, 이름: {nikkeGameData.name}, HP: {nikkeGameData.hp}");
            }
            Debug.Log(sb.ToString());
        }

        // --- 3. GameData와 UserData를 조합하여 최종 데이터 확인 ---
        if (Managers.Data.UserData.Nikkes != null)
        {
            Debug.Log("[종합 확인] 각 캐릭터의 최종 정보를 출력합니다.");
            StringBuilder sb = new StringBuilder();

            // UserData에 있는 모든 캐릭터의 상태 정보를 순회합니다.
            foreach (var userNikkeData in Managers.Data.UserData.Nikkes.Values)
            {
                // 캐릭터의 마스터 데이터(이름 등)를 GameData에서 가져옵니다.
                NikkeGameData gameData = Managers.Data.Get<NikkeGameData>(userNikkeData.id);

                if (gameData != null)
                {
                    sb.AppendLine($"  - 이름: {gameData.name} | 유저 레벨: {userNikkeData.level.Value}");
                }
            }
            Debug.Log(sb.ToString());
        }

        Debug.Log("======== 데이터 로드 결과 확인 완료 ========");


        Debug.Log("Test Scene Init() 합니다.");
        // ViewModel을 먼저 생성하고 UI 생성을 요청합니다.
        var viewModel = new PopupTestViewModel();
        _ = Managers.UI.ShowAsync<UI_PopupTest>(viewModel);
    }

    void IScene.Clear()
    {
        Debug.Log("Test Scene Clear() 합니다.");
    }
}