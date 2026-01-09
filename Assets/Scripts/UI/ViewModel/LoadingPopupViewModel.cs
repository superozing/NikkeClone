using System;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingPopupViewModel : ViewModelBase
{
    // 변경: 이미 실행된 Task가 아니라, 실행할 로직을 담은 델리게이트를 저장
    private readonly Func<Task> _workFunc;

    // View에게 연출을 요청하는 비동기 이벤트 (Func<Task> 사용)
    public event Func<Task> OnWipeInRequested;
    public event Func<Task> OnWipeOutRequested;

    // 로딩 종료 후 닫기 요청
    public event Action OnCloseRequested;

    public LoadingPopupViewModel(Func<Task> workFunc)
    {
        _workFunc = workFunc;
    }

    /// <summary>
    /// View의 초기화(Start 등) 시점에 호출되어 전체 로딩 프로세스를 실행합니다.
    /// </summary>
    public async void ExecuteProcess()
    {
        // 1. Wipe In 연출 시작 (화면을 덮음) - 완료될 때까지 대기
        if (OnWipeInRequested != null)
            await OnWipeInRequested.Invoke();

        // [Design Proposal] 등장 연출 후 전환이 어색하지 않도록 0.3초 대기
        await Task.Delay(300);

        try
        {
            // 2. 실제 작업 수행 (데이터 로드 + UI 생성)
            // 화면이 가려진 뒤에 실행됨
            if (_workFunc != null)
                await _workFunc.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LoadingPopupViewModel] 작업 수행 중 예외 발생: {ex}");
        }
        finally
        {
            // 3. 작업 성공/실패 여부와 관계없이 Wipe Out 연출 시작 (화면을 걷어냄)
            if (OnWipeOutRequested != null)
                await OnWipeOutRequested.Invoke();

            // 4. 모든 과정 종료 후 팝업 닫기
            OnCloseRequested?.Invoke();
        }
    }

    protected override void OnDispose()
    {
        OnWipeInRequested = null;
        OnWipeOutRequested = null;
        OnCloseRequested = null;
    }
}