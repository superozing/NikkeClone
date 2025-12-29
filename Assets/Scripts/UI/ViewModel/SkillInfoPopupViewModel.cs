using System;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class SkillInfoPopupViewModel : ViewModelBase
{
    public event Action OnCloseRequested;

    /// <summary>
    /// View에 바인딩 될 자식 슬롯 ViewModel 리스트입니다.
    /// 데이터 로드 시 항상 3개의 ViewModel이 채워질 것으로 가정합니다.
    /// </summary>
    public List<SkillSlotViewModel> SlotViewModels { get; private set; } = new();

    private NikkeGameData _gameData;

    public SkillInfoPopupViewModel()
    {
    }

    /// <summary>
    /// 팝업 데이터를 설정하고 자식 ViewModel을 생성합니다.
    /// </summary>
    /// <param name="nikkeId">대상 니케 ID</param>
    public void SetData(int nikkeId)
    {
        _gameData = Managers.Data.Get<NikkeGameData>(nikkeId);

        if (_gameData == null)
        {
            Debug.LogError($"[SkillInfoPopupViewModel] GameData를 찾을 수 없습니다. ID: {nikkeId}");
            return;
        }

        if (_gameData.skills == null || _gameData.skills.Count == 0)
        {
            Debug.LogWarning($"[SkillInfoPopupViewModel] 스킬 데이터가 비어있습니다. ID: {nikkeId}");
            return;
        }

        // 기존 슬롯 정리 (재사용 시)
        ClearSlots();

        // 스킬 데이터 순회하며 ViewModel 생성
        // 데이터상 스킬 개수가 3개라고 가정
        foreach (var skillData in _gameData.skills)
        {
            var slotVM = new SkillSlotViewModel(skillData);
            slotVM.AddRef();
            SlotViewModels.Add(slotVM);
        }
    }

    public void OnClickClose()
    {
        OnCloseRequested?.Invoke();
    }

    private void ClearSlots()
    {
        if (SlotViewModels != null)
        {
            foreach (var vm in SlotViewModels)
            {
                vm.Release();
            }
            SlotViewModels.Clear();
        }
    }

    protected override void OnDispose()
    {
        ClearSlots();
        OnCloseRequested = null;
    }
}