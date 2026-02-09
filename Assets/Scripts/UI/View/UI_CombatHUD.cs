using UnityEngine;
using UI;

/// <summary>
/// 전투 씬 HUD입니다.
/// 각 니케의 현재 상태를 표시하는 슬롯을 포함합니다.
/// </summary>
public class UI_CombatHUD : UI_View
{
    [Header("Nikke State Slots")]
    [SerializeField] private UI_NikkeStateSlot[] _nikkeStateSlots;  // 5개

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

    [Header("Phase 4: Wave Progress")]
    [SerializeField] private UnityEngine.UI.Image _progressFill; // Inspector에서 할당

    // Phase 4: WaveSystem 연동
    // Caller: CombatScene.Update()
    public void UpdateProgress(float progress)
    {
        if (_progressFill != null)
        {
            _progressFill.fillAmount = progress;
        }
    }

    // TODO Phase 5: 제한시간 표시
    // TODO Phase 8: 버스트 게이지/버튼
}
