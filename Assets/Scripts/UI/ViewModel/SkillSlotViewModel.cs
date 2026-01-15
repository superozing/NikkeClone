using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

public class SkillSlotViewModel : ViewModelBase
{
    private readonly SkillData _skillData;

    // --- View Binding Properties ---

    public ReactiveProperty<string> Name { get; private set; } = new("");
    public ReactiveProperty<string> Description { get; private set; } = new("");
    public ReactiveProperty<string> SkillType { get; private set; } = new(""); // "패시브" or "액티브"

    // 쿨타임 관련
    public ReactiveProperty<bool> IsCooldownVisible { get; private set; } = new(false);
    public ReactiveProperty<string> CooldownText { get; private set; } = new("");

    // 이미지
    public ReactiveProperty<Sprite> SkillIcon { get; private set; } = new();

    public SkillSlotViewModel(SkillData skillData)
    {
        _skillData = skillData;

        if (_skillData == null)
        {
            Debug.LogError("[SkillSlotViewModel] SkillData is null.");
            return;
        }

        InitializeData();
        LoadResources();
    }

    private void InitializeData()
    {
        Name.Value = _skillData.name;

        // 1. 스킬 타입 및 쿨타임 처리
        bool isActive = string.Equals(_skillData.skillTypeName, "액티브");

        SkillType.Value = _skillData.skillTypeName;
        IsCooldownVisible.Value = isActive;

        if (isActive)
        {
            // "20.0초" 형식
            CooldownText.Value = $"{_skillData.cooldown:F1}초";
        }

        // 2. 설명 텍스트 포맷팅
        // 설명 내의 {0}, {1} ... 을 values 리스트의 value 필드로 치환합니다.
        if (!string.IsNullOrEmpty(_skillData.description))
        {
            try
            {
                if (_skillData.values != null && _skillData.values.Count > 0)
                {
                    string[] valueArgs = _skillData.values.Select(v => v.value).ToArray();
                    Description.Value = string.Format(_skillData.description, valueArgs);
                }
                else
                {
                    // 치환할 값이 없으면 그대로 출력
                    Description.Value = _skillData.description;
                }
            }
            catch (FormatException ex)
            {
                // 포맷팅 실패 시 원본 텍스트 출력 및 로그
                Debug.LogWarning($"[SkillSlotViewModel] 설명 포맷팅 실패: {ex.Message}. 원본 텍스트를 사용합니다.");
                Description.Value = _skillData.description;
            }
        }
    }

    private async void LoadResources()
    {
        if (!string.IsNullOrEmpty(_skillData.skillIconPath))
        {
            SkillIcon.Value = await Managers.Resource.LoadAsync<Sprite>("DORO");// 스킬 아이콘 추가 전 까지 도로롱 사용할 예정이에요. // _skillData.skillIconPath);
        }
    }
}