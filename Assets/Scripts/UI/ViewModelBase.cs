using System;
using UI;
using UnityEngine;

public abstract class ViewModelBase : IDisposable
{
    private int _refCount = 0;
    private bool _boundOnce = false; // 생성 직후 바로 해제되는 것을 방지하기 위한 플래그

    /// <summary>
    /// View가 이 ViewModel을 참조하기 시작할 때(SetViewModel) 호출합니다.
    /// 또는 부모 ViewModel이 자식 ViewModel을 소유할 때 호출합니다.
    /// </summary>
    public void AddRef()
    {
        _boundOnce = true;
        _refCount++;
        // Debug.Log($"[ViewModelBase] {this.GetType().Name} AddRef. Count: {_refCount}");
    }

    /// <summary>
    /// View가 이 ViewModel의 참조를 해제할 때(OnDestroy, SetViewModel(null)) 호출합니다.
    /// 참조 카운트가 0이 되면 Dispose(OnDispose)를 호출합니다.
    /// </summary>
    public void Release()
    {
        // 한 번도 바인딩 된 적이 없다면(생성 직후) Release에 의해 파괴되지 않도록 보호
        if (!_boundOnce)
            return;

        _refCount--;
        // Debug.Log($"[ViewModelBase] {this.GetType().Name} Release. Count: {_refCount}");

        if (_refCount <= 0)
            Dispose();
    }

    public void Dispose()
    {
        OnDispose();
        // Debug.Log($"[ViewModelBase] {this.GetType().Name} Disposed.");
    }

    /// <summary>
    /// 자식 클래스에서 리소스 해제 및 자식 뷰모델 Release 로직을 구현해야 합니다.
    /// </summary>
    protected virtual void OnDispose() { }

}
