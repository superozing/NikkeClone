using System;
using UI;

public class NikkeTabViewModel : ViewModelBase
{
    // 자식 ViewModel
    public NikkeTabNikkeCardScrollViewModel ScrollViewModel { get; private set; }

    public NikkeTabViewModel()
    {
        // 스크롤 뷰모델 생성 및 소유
        ScrollViewModel = new NikkeTabNikkeCardScrollViewModel();
        ScrollViewModel.AddRef();
    }

    protected override void OnDispose()
    {
        if (ScrollViewModel != null)
        {
            ScrollViewModel.Release();
            ScrollViewModel = null;
        }
    }
}
