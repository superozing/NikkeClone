using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TestScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Test;
    public List<string> RequiredDataFiles => new() 
    { 
        "StatData.json", 
        "ItemData.json"
    };


    void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log("Test Scene Awake() 합니다.");
    }

    void IScene.Init()
    {
        Debug.Log(Application.persistentDataPath);
        Debug.Log("======== 데이터 로드 결과 확인 시작 ========");

        // --- 1. UserData 로드 확인 ---
        if (Managers.Data.UserData == null)
        {
            Debug.LogError("[확인 실패] UserData가 null입니다. UserData.json 파일이 없거나 손상되었습니다.");
            return;
        }
        else
        {
            Debug.Log($"[UserData] 골드: {Managers.Data.UserData.Gold.Value}");
            Debug.Log($"[UserData] 다이아: {Managers.Data.UserData.Dia.Value}");
        }

        // --- 2. GameData (StatData) 로드 확인 ---
        var statTable = Managers.Data.GetTable<StatData>();
        if (statTable == null)
        {
            Debug.LogError("[확인 실패] StatData 테이블이 로드되지 않았습니다.");
        }
        else
        {
            Debug.Log($"[GameData] StatData.json 로드 성공! 총 {statTable.Count}개의 데이터가 있습니다.");
            // StringBuilder를 사용하면 여러 문자열을 합칠 때 성능상 이점이 있습니다.
            StringBuilder sb = new StringBuilder();
            foreach (var stat in statTable.Values)
            {
                sb.AppendLine($"  - ID: {stat.ID}, 이름: {stat.name}, HP: {stat.maxHp}");
            }
            Debug.Log(sb.ToString());
        }

        // --- 3. GameData와 UserData를 조합하여 최종 데이터 확인 ---
        if (Managers.Data.UserData.Characters != null)
        {
            Debug.Log("[종합 확인] 각 캐릭터의 최종 정보를 출력합니다.");
            StringBuilder sb = new StringBuilder();

            // 유저가 보유한 모든 캐릭터의 상세 정보를 순회합니다.
            foreach (var userCharacter in Managers.Data.UserData.Characters.Values)
            {
                // 캐릭터의 마스터 데이터(이름 등)를 GameData에서 가져옵니다.
                StatData statData = Managers.Data.Get<StatData>(userCharacter.characterId);

                // 획득 여부를 UserData에서 확인합니다.
                bool isAcquired = Managers.Data.UserData.AcquiredCharacters.Contains(userCharacter.characterId);

                sb.AppendLine($"  - 이름: {statData.name} | 레벨: {userCharacter.level.Value} | 획득 여부: {isAcquired}");
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