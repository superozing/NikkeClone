using System;
using System.Threading.Tasks;
using UnityEngine;

public class NikkeTabNikkeCardScrollViewModel : NikkeCardScrollViewModelBase
{
    protected override async void OnCardClick(int nikkeId)
    {
        // 화면이 가려진 후 실행될 로직
        Func<Task> loadTask = async () =>
        {
            // 1. 팝업 뷰모델 생성
            NikkeDetailPopupViewModel popupVM = new NikkeDetailPopupViewModel();

            // 2. 데이터 설정 및 리소스 로드 (완료될 때까지 대기)
            await popupVM.SetNikkeID(nikkeId);

            // 3. 팝업 표시
            await Managers.UI.ShowAsync<UI_NikkeDetailPopup>(popupVM);
        };

        // 4. 로딩 팝업 생성 및 표시
        // UI_LoadingPopup은 DontDestroyPopup이므로 ShowDontDestroyAsync 사용
        var loadingVM = new LoadingPopupViewModel(loadTask);
        await Managers.UI.ShowDontDestroyAsync<UI_LoadingPopup>(loadingVM);
    }
}