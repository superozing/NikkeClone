using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;

/// <summary>
/// 버스트 게이지를 시각화하는 클래스입니다.
/// </summary>
public class UI_BurstGauge : UI_View
{
    [Header("Colors")]
    [SerializeField] private Color[] _stageColors; // 0:None, 1:Stage1, 2:Stage2, 3:Stage3, 4:FullBurst

    [Header("Visual Elements")]
    [SerializeField] private Image _gaugeFill;
    [SerializeField] private GameObject[] _burstStageObjects; // 0:None, 1:Stage1, 2:Stage2, 3:Stage3, 4:FullBurst

    private BurstGaugeViewModel _burstViewModel;

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _burstViewModel = viewModel as BurstGaugeViewModel;
        base.SetViewModel(viewModel);

        if (_burstViewModel == null) return;

        // 게이지 바인딩
        Bind(_burstViewModel.Gauge, OnGaugeChanged);

        // 단계 바인딩
        Bind(_burstViewModel.CurrentStage, OnStageChanged);
    }

    private void OnGaugeChanged(float val)
    {
        if (_gaugeFill != null)
            _gaugeFill.fillAmount = val;
    }

    private void OnStageChanged(NikkeClone.Utils.eBurstStage stage)
    {
        if (_gaugeFill == null) return;

        // 1. 색상 변경
        _gaugeFill.color = _stageColors[(int)stage];

        // 2. 단계 오브젝트 제어 (아이콘 및 이펙트 통합 제어)
        if (_burstStageObjects != null)
        {
            int currentStageIdx = (int)stage;
            bool isNone = stage == NikkeClone.Utils.eBurstStage.None;

            for (int i = 0; i < _burstStageObjects.Length; i++)
            {
                if (_burstStageObjects[i] == null) continue;

                // 0단계(None)일 경우 모든 아이콘 비활성화, 그 외에는 해당 단계만 활성화
                if (isNone)
                    _burstStageObjects[i].SetActive(false);
                else
                    _burstStageObjects[i].SetActive(i == currentStageIdx);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _burstViewModel = null;
    }
}
