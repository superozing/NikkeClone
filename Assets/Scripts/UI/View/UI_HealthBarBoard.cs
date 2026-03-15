using System.Collections.Generic;
using UnityEngine;
using UI;

/// <summary>
/// 모든 엔터티의 체력바를 한 곳(Screen Space)에서 관리하는 보드입니다.
/// </summary>
public class UI_HealthBarBoard : UI_View
{
    private readonly Dictionary<CombatEntity, WorldHealthBar> _activeHealthBars = new();
    private RectTransform _rectTransform;
    private Camera _mainCamera;

    protected override void Awake()
    {
        base.Awake();
        _rectTransform = GetComponent<RectTransform>();
        _mainCamera = Camera.main;
    }

    /// <summary>
    /// 엔터티를 등록하여 체력바를 생성합니다.
    /// </summary>
    public async void RegisterEntity(CombatEntity entity)
    {
        if (entity == null || _activeHealthBars.ContainsKey(entity)) return;

        // WorldHealthBar 생성 (UIManager를 통하거나 직접 Instantiate)
        // 여기서는 프로젝트 컨벤션에 맞춰 Managers.UI.ShowAsync 구조를 활용할 수 있으나, 
        // 대량 생성이므로 하위 요소로서 관리합니다.

        GameObject go = await Managers.Resource.InstantiateAsync("UI/View/WorldHealthBar", parent: _rectTransform);
        WorldHealthBar healthBar = go.GetComponent<WorldHealthBar>();

        // 초기 설정
        healthBar.SetTrackingTarget(entity.HealthBarTrackingAnchor, _mainCamera, _rectTransform);
        healthBar.SetViewModel(new EntityHealthViewModel(entity));

        _activeHealthBars.Add(entity, healthBar);
    }

    /// <summary>
    /// 엔터티 등록을 해제하고 체력바를 파괴(풀링 반환)합니다.
    /// </summary>
    public void UnregisterEntity(CombatEntity entity)
    {
        if (entity == null || !_activeHealthBars.ContainsKey(entity)) return;

        WorldHealthBar healthBar = _activeHealthBars[entity];
        _activeHealthBars.Remove(entity);

        if (healthBar != null)
        {
            Managers.Resource.Destroy(healthBar.gameObject);
        }
    }

    public void Clear()
    {
        foreach (var hb in _activeHealthBars.Values)
        {
            if (hb != null) Managers.Resource.Destroy(hb.gameObject);
        }
        _activeHealthBars.Clear();
    }
}
