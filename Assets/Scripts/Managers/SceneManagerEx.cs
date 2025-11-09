using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEx : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Scene;
    public IScene CurrentScene { get; private set; }

    /// <summary>
    /// 새 씬 로드 시 씬이 자신을 등록할 때 사용합니다.
    /// </summary>
    /// <param name="scene">새로 로드된 씬 자신</param>
    public void SetCurrentScene(IScene scene) => CurrentScene = scene;

    public void Init()
    {
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Start()
    {
        Debug.Log($"{ManagerType} Manager Start 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        Debug.Log($"{ManagerType} Manager Clear 합니다.");

        // 현재 씬 Clear
        CurrentScene?.Clear();
        CurrentScene = null;
    }

    //////////////

    /// <summary>
    /// 비동기로 씬(Scene)을 로드합니다.
    /// IProgress<float>를 통해 진행률을 보고합니다.
    /// </summary>
    /// <param name="sceneType">로드할 씬 타입</param>
    /// <param name="progress">진행률을 보고받을 IProgress<float> 구현체입니다.</param>
    public async Task LoadSceneAsync(eSceneType sceneType, IProgress<float> progress = null)
    {
        // 1. 로딩 시작
        progress?.Report(0f);

        // 2. 이전 씬의 모든 리소스 정리
        Managers.Inst.Clear();

        string sceneName = sceneType.ToString();

        // 3. 씬 비동기 로드 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 4. operation.progress가 0.9f에 도달할 때까지 진행률을 갱신합니다.
        while (operation.progress < 0.9f)
        {
            progress?.Report(operation.progress); // 0.0 ~ 0.9 사이의 값 보고
            await Task.Yield(); // 1프레임 대기
        }
        progress?.Report(0.9f); // 90%로 고정

        // 5. 씬 활성화 및 완료 대기
        // 여기서 testScene Awake가 호출
        operation.allowSceneActivation = true;
        await operation;

        if (CurrentScene == null)
        {
            Debug.LogError($"[SceneManagerEx] 씬(Scene) 로드 실패: {sceneName}의 IScene이 null입니다.");
            progress?.Report(1f);
            return;
        }

        // 6. 데이터 로드, 매니저 start 호출
        await Managers.Inst.StartSceneAsync();

        // 7. 로딩 완료
        progress?.Report(1.0f);
    }
}
