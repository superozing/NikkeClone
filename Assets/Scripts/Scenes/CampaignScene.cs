using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UI;

public class CampaignScene : MonoBehaviour, IScene
{
    eSceneType IScene.SceneType => eSceneType.CampaignScene;
    public string DefaultActionMapKey => "None";
    public List<string> RequiredDataFiles => new()
    {
        "ChapterGameData.json",
        "StageGameData.json",
        "MissionGameData.json",
        "NikkeGameData.json",
        "ItemGameData.json",
    };

    private UI_CampaignHUD _campaignHUD;
    private int _currentCombatStageId = -1;

    [Header("챕터 정보")]
    [SerializeField] private int _chapterId;
    [SerializeField] private CampaignStage[] _stageObjects;

    // ViewModel 캐시
    private StageInfoPopupViewModel _stageInfoPopupViewModel;
    private SquadDetailPopupViewModel _squadDetailPopupViewModel;
    private CampaignHUDViewModel _campaignHUDViewModel;

    [Header("스쿼드")]
    [SerializeField] private CampaignSquad _squad;

    /// <summary>
    /// 스테이지 전투 상태 진입 시 호출됩니다.
    /// HUD 퇴장 연출 후 StageInfoPopup을 표시합니다.
    /// </summary>
    /// <param name="stageId">전투 진입한 스테이지 ID</param>
    public async void OnStageEnterCombat(int stageId)
    {
        _currentCombatStageId = stageId;

        // 1. HUD 퇴장
        if (_campaignHUD != null)
            await _campaignHUD.PlayHideAnimationAsync();

        // 2. 캐싱된 ViewModel 재사용 및 데이터 갱신
        if (_stageInfoPopupViewModel == null)
        {
            Debug.LogError("[CampaignScene] StageInfoPopupViewModel이 초기화되지 않았습니다!");
            return;
        }

        // TODO: 현재 유저의 스쿼드 ID를 가져오는 로직 필요 (임시로 1 사용)
        int currentSquadId = 1;
        await _stageInfoPopupViewModel.Initialize(stageId, currentSquadId);

        // 3. 팝업 표시 (동일한 ViewModel 인스턴스 재사용)
        await Managers.UI.ShowAsync<UI_StageInfoPopup>(_stageInfoPopupViewModel);
    }

    /// <summary>
    /// StageInfoPopup이 닫혔을 때 호출됩니다.
    /// HUD 등장 연출을 수행합니다.
    /// </summary>
    public async void OnStageInfoPopupClosed()
    {
        // 1. HUD 등장
        if (_campaignHUD != null)
            await _campaignHUD.PlayShowAnimationAsync();

        // 2. 전투 상태 탈출
        if (_currentCombatStageId != -1 && _stageObjects != null)
        {
            CampaignStage targetStage = null;
            foreach (var stage in _stageObjects)
            {
                if (stage != null && stage.StageId == _currentCombatStageId)
                {
                    stage.ExitCombat();
                    targetStage = stage;
                    break;
                }
            }

            // Bug #5 Fix: 스쿼드를 스테이지 시선 방향으로 이동시켜 충돌체에서 벗어나게 함
            if (_squad != null && targetStage != null)
            {
                _squad.ExitCombat(targetStage.ForwardDirection);
            }

            _currentCombatStageId = -1;
        }
    }

    /// <summary>
    /// 스쿼드 편집(디테일) 팝업 요청 처리
    /// </summary>
    private void OnSquadEditRequested()
    {
        // 기존 팝업 정리
        CloseSquadDetailPopup();

        // 현재 선택된 스쿼드 인덱스를 넘겨주며 생성
        int currentSquadIdx = (_stageInfoPopupViewModel != null) ? _stageInfoPopupViewModel.CurrentSquadIndex.Value : 0;

        _squadDetailPopupViewModel = new SquadDetailPopupViewModel(currentSquadIdx);
        _squadDetailPopupViewModel.AddRef();

        _squadDetailPopupViewModel.OnCloseRequested += OnSquadDetailPopupClosed;

        _ = Managers.UI.ShowAsync<UI_SquadDetailPopup>(_squadDetailPopupViewModel);
    }

    private async void OnSquadDetailPopupClosed()
    {
        // 팝업 닫기
        CloseSquadDetailPopup();

        // 스쿼드 변경사항이 있을 수 있으므로 StageInfoPopup 갱신
        if (_stageInfoPopupViewModel != null)
        {
            await _stageInfoPopupViewModel.RefreshSquads();
        }
    }

    private void CloseSquadDetailPopup()
    {
        if (_squadDetailPopupViewModel != null)
        {
            _squadDetailPopupViewModel.OnCloseRequested -= OnSquadDetailPopupClosed;
            _squadDetailPopupViewModel.Release();
            _squadDetailPopupViewModel = null;
        }
    }


    private void Awake()
    {
        Managers.Scene.SetCurrentScene(this);
        Debug.Log($"CampaignScene Awake() - ChapterId: {_chapterId}");
    }

    private async void InitializeHUD()
    {
        _campaignHUD = await Managers.UI.ShowAsync<UI_CampaignHUD>(_campaignHUDViewModel);
        if (_campaignHUD != null)
        {
            await _campaignHUD.PlayShowAnimationAsync();
        }
    }

    async Task IScene.InitAsync()
    {
        Debug.Log($"CampaignScene InitAsync() - ChapterId: {_chapterId}");

        // ViewModel 생성 및 명시적 참조 카운트 증가
        _stageInfoPopupViewModel = new StageInfoPopupViewModel();
        _stageInfoPopupViewModel.AddRef(); // CampaignScene이 소유 (RefCount = 1)

        // HUD ViewModel 생성 및 연결
        _campaignHUDViewModel = new CampaignHUDViewModel();
        _campaignHUDViewModel.AddRef();

        // HUD 동적 생성 요청 (비동기)
        InitializeHUD();

        // 이벤트 구독 (한 번만)
        _stageInfoPopupViewModel.OnCloseRequested += OnStageInfoPopupClosed;
        _stageInfoPopupViewModel.OnSquadEditRequested += OnSquadEditRequested;

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

            // TODO: 전투 승리 후 캠페인 복귀 시 dead 연출을 재생하려면,
            //       Init()이 아닌 별도 시점(e.g. 복귀 직후 플래그 기반)에서 Die()를 트리거해야 합니다.
            //       Init() 시점에서는 즉시 비활성화하여 안정성을 확보합니다.
            _stageObjects[i].gameObject.SetActive(!isCleared);
        }

        Debug.Log($"[CampaignScene] CampaignScene InitAsync() 완료");
        await Task.CompletedTask;
    }

    void IScene.Clear()
    {
        Debug.Log($"CampaignScene Clear() - ChapterId: {_chapterId}");

        // 이벤트 구독 해제 및 ViewModel Release
        if (_stageInfoPopupViewModel != null)
        {
            _stageInfoPopupViewModel.OnCloseRequested -= OnStageInfoPopupClosed;
            _stageInfoPopupViewModel.OnSquadEditRequested -= OnSquadEditRequested;
            _stageInfoPopupViewModel.Release(); // RefCount = 0 → OnDispose() 호출됨
            _stageInfoPopupViewModel = null;
        }

        // HUD 정리
        if (_campaignHUD != null)
        {
            Managers.UI.Close(_campaignHUD);
            _campaignHUD = null;
        }

        // HUD ViewModel 정리
        if (_campaignHUDViewModel != null)
        {
            _campaignHUDViewModel.Release();
            _campaignHUDViewModel = null;
        }

        // SquadDetailPopup 정리
        CloseSquadDetailPopup();
    }
}
