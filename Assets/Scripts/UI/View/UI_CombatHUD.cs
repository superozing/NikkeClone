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
            return;


        // 각 슬롯에 니케 상태 바인딩
        for (int i = 0; i < _nikkeStateSlots.Length; i++)
        {
            // TODO: CombatHUD뷰모델이 생성할 NikkeState 뷰모델에 니케 아이디 세팅하는 로직 필요
            // 여기서는 SetViewModel만 호출해주어야 한다.
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

    /// <summary>
    /// 현재 조작 중인 니케 슬롯을 하이라이트합니다.
    /// Caller: CombatScene.SwitchNikke()
    /// </summary>
    public void SetActiveNikkeSlot(int slotIndex)
    {
        if (_nikkeStateSlots == null) return;

        for (int i = 0; i < _nikkeStateSlots.Length; i++)
        {
            if (_nikkeStateSlots[i] != null)
            {
                _nikkeStateSlots[i].SetControlled(i == slotIndex);
            }
        }
    }



    /// <summary>
    /// 남은 시간을 갱신합니다.
    /// Caller: CombatScene.Update()
    /// </summary>
    /// <param name="remainingSec">남은 시간(초)</param>
    public void UpdateTimer(float remainingSec)
    {
        if (_txtTimer == null) return;

        // 음수 방지
        remainingSec = Mathf.Max(0, remainingSec);

        int minutes = Mathf.FloorToInt(remainingSec / 60);
        int seconds = Mathf.FloorToInt(remainingSec % 60);

        _txtTimer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
