using System.Collections.Generic;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class UI_SquadTab : UI_TabBase
{
    public override eTabType TabType => eTabType.Squad;

    [Header("Squad Selection")]
    [SerializeField] private Button[] _squadButtons; // 스쿼드 1~5 선택 버튼

    [Header("Info")]
    [SerializeField] private TMP_Text _combatPowerText; // 전투력 텍스트

    [Header("Slots")]
    [SerializeField] private UI_NikkeCard[] _cardSlots; // 니케 카드 슬롯 5개
    [SerializeField] private Button[] _skillButtons;    // 스킬 정보 버튼 5개
    [SerializeField] private Button[] _detailButtons;   // 상세 정보 버튼 5개

    [Header("Actions")]
    [SerializeField] private Button _autoButton; // 자동 편성 버튼

    private SquadTabViewModel _viewModel;

    protected override void Awake()
    {
        base.Awake();

        // 1. 스쿼드 선택 버튼 리스너 등록
        for (int i = 0; i < _squadButtons.Length; i++)
        {
            int index = i; // 클로저 캡처
            _squadButtons[i].onClick.AddListener(() => _viewModel?.OnClickSquadButton(index));
        }

        // 2. 스킬 팝업 버튼 리스너 등록
        for (int i = 0; i < _skillButtons.Length; i++)
        {
            int index = i;
            _skillButtons[i].onClick.AddListener(() => _viewModel?.OnClickSkill(index));
        }

        // 3. 상세 정보 버튼 리스너 등록
        for (int i = 0; i < _detailButtons.Length; i++)
        {
            int index = i;
            _detailButtons[i].onClick.AddListener(() => _viewModel?.OnClickDetail(index));
        }

        // 4. 자동 편성 버튼 리스너 등록
        _autoButton.onClick.AddListener(() => _viewModel?.OnClickAutoFormation());
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        _viewModel = viewModel as SquadTabViewModel;

        // 부모의 SetViewModel 호출 (기존 바인딩 해제 등)
        base.SetViewModel(viewModel);

        if (_viewModel != null)
        {
            // ReactiveProperty 구독
            Bind(_viewModel.TotalCombatPower, text => _combatPowerText.text = text);
            Bind(_viewModel.CurrentSquadIndex, OnSquadIndexChanged);

            // 초기 상태 반영
            OnSquadIndexChanged(_viewModel.CurrentSquadIndex.Value);
        }
    }

    /// <summary>
    /// 스쿼드 인덱스가 변경되었을 때 호출됩니다.
    /// 버튼 상태를 갱신하고 슬롯(카드, 버튼)을 다시 그립니다.
    /// </summary>
    private void OnSquadIndexChanged(int index)
    {
        // 1. 스쿼드 버튼 Interactable 상태 갱신 (선택된 버튼만 비활성화)
        for (int i = 0; i < _squadButtons.Length; i++)
        {
            if (_squadButtons[i] != null)
                _squadButtons[i].interactable = (i != index);
        }

        // 2. 슬롯 데이터 갱신
        UpdateSlots();
    }

    private void UpdateSlots()
    {
        if (_viewModel == null || _viewModel.SlotViewModels == null)
            return;

        // 슬롯 개수(5개)만큼 순회
        for (int i = 0; i < 5; i++)
        {
            // 해당 슬롯의 ViewModel 가져오기 (비어있으면 null)
            var vm = _viewModel.SlotViewModels[i];
            bool hasNikke = (vm != null);

            // 카드 슬롯 설정
            if (i < _cardSlots.Length && _cardSlots[i] != null)
            {
                _cardSlots[i].SetViewModel(vm);
                _cardSlots[i].gameObject.SetActive(hasNikke);
            }

            // 스킬 버튼 활성/비활성
            if (i < _skillButtons.Length && _skillButtons[i] != null)
            {
                _skillButtons[i].gameObject.SetActive(hasNikke);
            }

            // 상세 버튼 활성/비활성
            if (i < _detailButtons.Length && _detailButtons[i] != null)
            {
                _detailButtons[i].gameObject.SetActive(hasNikke);
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 버튼 리스너 해제
        foreach (var btn in _squadButtons) if (btn != null) btn.onClick.RemoveAllListeners();
        foreach (var btn in _skillButtons) if (btn != null) btn.onClick.RemoveAllListeners();
        foreach (var btn in _detailButtons) if (btn != null) btn.onClick.RemoveAllListeners();
        if (_autoButton != null) _autoButton.onClick.RemoveAllListeners();

        // ViewModel 해제는 Base에서 처리됨
        _viewModel = null;
    }
}