using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class CampaignScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.Campaign;
    public List<string> RequiredDataFiles => new()
    {
        "ChapterGameData.json",
        "StageGameData.json",
        "MissionGameData.json",
        "NikkeGameData.json",
        "ItemGameData.json",
    };

    [Header("UI")]
    // [SerializeField] private UI_CampaignHUD _campaignHUD;
    private int _currentCombatStageId = -1;

    [Header("챕터 정보")]
    [SerializeField] private int _chapterId;
    [SerializeField] private CampaignStage[] _stageObjects;

    /// <summary>
    /// 스테이지 전투 상태 진입 시 호출됩니다.
    /// HUD 퇴장 연출 후 StageInfoPopup을 표시합니다.
    /// </summary>
    /// <param name="stageId">전투 진입한 스테이지 ID</param>
    public async void OnStageEnterCombat(int stageId)
    {
        _currentCombatStageId = stageId;

        // 1. HUD 퇴장
        // if (_campaignHUD != null)
        //     await _campaignHUD.PlayExitAnimationAsync();

        // 2. ViewModel 생성 및 초기화
        var viewModel = new StageInfoPopupViewModel();

        // TODO: 현재 유저의 스쿼드 ID를 가져오는 로직 필요 (임시로 1 사용)
        int currentSquadId = 1;
        await viewModel.Initialize(stageId, currentSquadId);

        // 3. 이벤트 구독 (다시 닫힐 때를 위함)
        viewModel.OnCloseRequested += OnStageInfoPopupClosed;

        // 4. 팝업 표시
        await Managers.UI.ShowAsync<UI_StageInfoPopup>(viewModel);
    }

    /// <summary>
    /// StageInfoPopup이 닫혔을 때 호출됩니다.
    /// HUD 등장 연출을 수행합니다.
    /// </summary>
    public async void OnStageInfoPopupClosed()
    {
        // 1. HUD 등장
        // if (_campaignHUD != null)
        //     await _campaignHUD.PlayEnterAnimationAsync();

        // 2. 전투 상태 탈출
        if (_currentCombatStageId != -1 && _stageObjects != null)
        {
            foreach (var stage in _stageObjects)
            {
                if (stage != null && stage.StageId == _currentCombatStageId)
                {
                    stage.ExitCombat();
                    break;
                }
            }
            _currentCombatStageId = -1;
        }
    }

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
        // 스테이지 ID 할당 및 클리어 여부에 따른 오브젝트 활성화 조절
        for (int i = 0; i < _stageObjects.Length; i++)
        {
            _stageObjects[i].SetStageId(curChapter.stageIds[i]);
            bool isCleared = userData.IsStageCleared(curChapter.stageIds[i]);

            _stageObjects[i].gameObject.SetActive(!isCleared);
        }

        Debug.Log($"[CampaignScene] CampaignScene Init() 완료");
    }

    void IScene.Clear()
    {
        Debug.Log($"CampaignScene Clear() - ChapterId: {_chapterId}");
    }
}
