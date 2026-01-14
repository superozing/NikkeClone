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
        await OnWipeInRequested.Invoke();

        await _workFunc.Invoke();
        await Task.Delay(1);
        
        await OnWipeOutRequested.Invoke();

        OnCloseRequested?.Invoke();
    }

    protected override void OnDispose()
    {
        OnWipeInRequested = null;
        OnWipeOutRequested = null;
        OnCloseRequested = null;
    }
}