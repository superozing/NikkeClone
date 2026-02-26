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
    [SerializeField] private Image _progressFill;

    [Header("Phase 9: Burst UI")]
    [SerializeField] private UI_BurstGauge _burstGauge;
    [SerializeField] private UI_BurstSkillSlot[] _burstSkillSlots; // 5개

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

            // Phase 9: Burst UI Binding
            Bind(_viewModel.BurstGauge, burstGaugeVM =>
            {
                if (_burstGauge != null)
                {
                    _burstGauge.SetViewModel(burstGaugeVM);
                }

                if (burstGaugeVM != null && _burstSkillSlots != null)
                {
                    for (int i = 0; i < _burstSkillSlots.Length; i++)
                    {
                        if (_burstSkillSlots[i] == null) continue;

                        var slotVM = (i < burstGaugeVM.SlotViewModels.Count) ? burstGaugeVM.SlotViewModels[i] : null;
                        _burstSkillSlots[i].SetViewModel(slotVM);
                    }
                }
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
