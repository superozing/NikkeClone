using System.Collections.Generic;
using UnityEngine;

public class CampaignScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Campaign;
    public List<string> RequiredDataFiles => new()
    {
        "ChapterGameData.json",
        "StageGameData.json"
    };

    [Header("챕터 정보")]
    [SerializeField] private int _chapterId;
    [SerializeField] private GameObject[] _stageObjects; // TODO: 게임 오브젝트 대신, 스테이지 컨트롤러? 같은 스크립트를 받아오도록 수정해야 함.

    private void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log($"CampaignScene Awake() - ChapterId: {_chapterId}");
    }

    void IScene.Init()
    {
        Debug.Log($"CampaignScene Init() - ChapterId: {_chapterId}");

        // 챕터 데이터 조회
        ChapterGameData curChapter = Managers.Data.Get<ChapterGameData>(_chapterId);
        UserChapterData userData = Managers.Data.UserData.Chapter;

        if (curChapter == null)
        {
            Debug.LogError($"[CampaignScene] ChapterId({_chapterId})에 해당하는 ChapterGameData를 찾을 수 없습니다.");
            return;
        }
        if (_stageObjects.Length != curChapter.stageIds.Length)
        {
            Debug.LogError($"[CampaignScene] _stageObjects 배열 길이({_stageObjects.Length})와 ChapterGameData.stageIds 배열 길이({curChapter.stageIds.Length})가 일치하지 않습니다.");
            return;
        }

        // =========================================================
        // 클리어 여부에 따른 챕터 오브젝트 활성화 조절
        for (int i = 0; i < _stageObjects.Length; i++)
        {
            bool isCleared = userData.IsStageCleared(curChapter.stageIds[i]);

            _stageObjects[i].SetActive(!isCleared);
        }

        Debug.Log($"[CampaignScene] CampaignScene Init() 완료");
    }

    void IScene.Clear()
    {
        Debug.Log($"CampaignScene Clear() - ChapterId: {_chapterId}");
    }
}
