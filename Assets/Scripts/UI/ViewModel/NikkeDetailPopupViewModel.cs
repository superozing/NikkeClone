using System;
using System.Threading.Tasks;
using UnityEngine;

public class NikkeDetailPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;

    // --- Sub ViewModels ---
    public MoneyViewModel MoneyViewModel { get; private set; }

    // StatusViewModel은 니케 ID에 따라 교체되므로 ReactiveProperty로 관리하여 View가 구독하게 합니다.
    public ReactiveProperty<NikkeDetailStatusViewModel> StatusViewModel { get; private set; } = new();

    // --- Popup Data ---
    public ReactiveProperty<Sprite> NikkeStandingImage { get; private set; } = new();
    public ReactiveProperty<Color> ThemeColor { get; private set; } = new(Color.white);

    public NikkeDetailPopupViewModel()
    {
        // MoneyViewModel은 팝업 수명주기 동안 유지됩니다.
        MoneyViewModel = new MoneyViewModel();
        MoneyViewModel.AddRef();
    }

    /// <summary>
    /// 표시할 니케 ID를 설정합니다.
    /// </summary>
    public async Task SetNikkeID(int nikkeId)
    {
        var gameData = Managers.Data.Get<NikkeGameData>(nikkeId);
        if (!Managers.Data.UserData.Nikkes.TryGetValue(nikkeId, out var userData))
        {
            Debug.LogError($"[NikkeDetailPopupViewModel] UserData not found for ID: {nikkeId}");
            return;
        }

        if (gameData == null)
        {
            Debug.LogError($"[NikkeDetailPopupViewModel] GameData not found for ID: {nikkeId}");
            return;
        }

        // 1. 기존 StatusViewModel 정리
        StatusViewModel.Value = new NikkeDetailStatusViewModel(gameData, userData);;

        // 2. 테마 색상 설정
        ThemeColor.Value = gameData.color;

        // 3. 니케 이미지 로드
        string path = $"Assets/Textures/Nikke/{gameData.name}_Stand";
        Sprite sprite = await Managers.Resource.LoadAsync<Sprite>(path);

        if (sprite != null)
            NikkeStandingImage.Value = sprite;
        else
            Debug.LogError($"[NikkeDetailPopupViewModel] Standing Image not found: {path}");
    }

    public void OnClickClose()
    {
        OnCloseRequested?.Invoke();
    }

    protected override void OnDispose()
    {
        if (MoneyViewModel != null)
        {
            MoneyViewModel.Release();
            MoneyViewModel = null;
        }

        OnCloseRequested = null;
    }
}