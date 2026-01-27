using System.Threading.Tasks;
using UnityEngine;

public class RaptureIconViewModel : ViewModelBase
{
    public int RaptureId { get; private set; }
    public ReactiveProperty<Sprite> RaptureSprite { get; private set; } = new(null);
    public ReactiveProperty<Sprite> CodeSprite { get; private set; } = new(null);
    public int Grade { get; private set; }

    /// <summary>
    /// RaptureIconViewModel 생성자.
    /// </summary>
    /// <param name="raptureId">랩쳐 ID</param>
    public RaptureIconViewModel(int raptureId)
    {
        RaptureId = raptureId;

        var raptureData = Managers.Data.Get<RaptureGameData>(raptureId);
        if (raptureData == null)
        {
            Debug.LogError($"[RaptureIconViewModel] RaptureGameData not found for ID: {raptureId}");
            return;
        }

        Grade = raptureData.grade;
        LoadResourcesAsync(raptureData); // 생성자에서는 비동기 작업을 수행할 수 없어요. 그렇기에 함수 분리
    }

    private async void LoadResourcesAsync(RaptureGameData raptureData)
    {
        // 랩쳐 이미지 로드
        string raptureResPath = $"Assets/Textures/Rapture/{raptureData.name}.png";
        RaptureSprite.Value = await Managers.Resource.LoadAsync<Sprite>(raptureResPath);

        // 속성코드 이미지 로드
        string codePath = $"Assets/Textures/Icon/Code/{(eNikkeCode)raptureData.elementCode}";
        CodeSprite.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);
    }

    public void OnClick()
    {
        if (RaptureId == 0) return;

        Debug.Log($"[RaptureIconViewModel] Clicked Rapture ID: {RaptureId}");

        // TODO: UI_StageEnemyInfoPopup 띄우기
    }
}
