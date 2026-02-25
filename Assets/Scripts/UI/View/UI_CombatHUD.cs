using UnityEngine;
using UI;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 전투 씬 HUD입니다.
/// 각 니케의 현재 상태를 표시하는 슬롯을 포함합니다.
/// </summary>
public class UI_CombatHUD : UI_View
{
    [Header("Phase 6: Timer & Pause")]
    [SerializeField] private TMP_Text _txtTimer;
    [SerializeField] private Button _btnPause;

    [Header("Nikke State Slots")]
    [SerializeField] private UI_NikkeStateSlot[] _nikkeStateSlots;  // 5개
    [Header("Phase 4: Wave Progress")]
    [SerializeField] private Image _progressFill; // Inspector에서 할당

    private CombatHUDViewModel _viewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as CombatHUDViewModel;

        if (_viewModel == null && viewModel != null)
        {
            Debug.LogError($"[UI_CombatHUD] 올바르지 않은 뷰모델 타입입니다:{viewModel.GetType()}");
            return;
        }

        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // 각 슬롯에 니케 상태 바인딩
            if (_viewModel.Nikkes != null)
            {
                for (int i = 0; i < _nikkeStateSlots.Length; i++)
                {
                    if (_nikkeStateSlots[i] == null) continue;

                    CombatNikke nikke = (i < _viewModel.Nikkes.Length) ? _viewModel.Nikkes[i] : null;
                    var slotViewModel = new NikkeStateViewModel(nikke);
                    _nikkeStateSlots[i].SetViewModel(slotViewModel);
                }
            }

            // Timer Binding
            Bind(_viewModel.TimeText, timeStr =>
            {
                if (_txtTimer != null) _txtTimer.text = timeStr;
            });
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _viewModel = null;
    }


    // Phase 4: WaveSystem 연동
    // Caller: CombatScene.Update()
    public void UpdateProgress(float progress)
    {
        if (_progressFill != null)
        {
            _progressFill.fillAmount = progress;
        }
    }
}
