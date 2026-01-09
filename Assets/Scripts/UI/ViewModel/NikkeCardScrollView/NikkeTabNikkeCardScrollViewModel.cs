using System.Threading.Tasks;
using UnityEngine;

public class NikkeTabNikkeCardScrollViewModel : NikkeCardScrollViewModelBase
{
    protected override async void OnCardClick(int nikkeId)
    {
        // 1. 팝업 뷰모델 생성
        NikkeDetailPopupViewModel popupVM = new NikkeDetailPopupViewModel();

        // 2. 데이터 설정 및 리소스 로드 (완료될 때까지 대기)
        await popupVM.SetNikkeID(nikkeId);

        // 3. 팝업 표시
        await Managers.UI.ShowAsync<UI_NikkeDetailPopup>(popupVM);
    }
}