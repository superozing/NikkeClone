using UnityEngine;

/// <summary>
/// 공격 상태. Weapon.CurrentAimScreenPosition(CombatNikke가 매 프레임 갱신)을 읽어
/// Raycast 후 IWeapon에 사격을 위임합니다.
/// Strategy 관련 로직은 CombatNikke에 있습니다.
/// </summary>
public class NikkeAttackState : IState<CombatNikke>
{
    private static readonly int CombatLayerMask
        = LayerMask.GetMask("CombatRapture", "CombatObstacle");

    public void Enter(CombatNikke owner)
    {
        owner.View.UpdateVisualState(eNikkeState.Attack);
        owner.Weapon?.Enter(owner);
    }

    public void Execute(CombatNikke owner)
    {
        var camera = owner.CachedCamera;
        if (camera == null || owner.Weapon == null) return;

        // (1) CombatNikke.UpdateAimPosition()이 매 프레임 기록한 스크린 좌표를 읽음
        Vector2 aimScreenPos = owner.Weapon.CurrentAimScreenPosition.Value;

        // (2) 스크린 좌표 → 월드 좌표 변환 + Rapture 판정
        Ray ray = camera.ScreenPointToRay(aimScreenPos);
        bool isHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, CombatLayerMask);

        CombatRapture rapture = null;
        Vector3 targetWorldPos;

        if (isHit)
        {
            targetWorldPos = hit.point;
            rapture = hit.collider.GetComponentInParent<CombatRapture>();
        }
        else
        {
            targetWorldPos = ray.GetPoint(100f);
        }

        // (3) 적정 사거리 피드백
        owner.Weapon.IsInPreferredZone.Value =
            rapture != null && owner.Weapon.IsPreferredZone(rapture.CurrentZone);

        // (4) 사격 판단을 무기에 위임
        owner.Weapon.ProcessCombat(owner, targetWorldPos, rapture != null);
    }

    public void Exit(CombatNikke owner)
    {
        owner.Weapon?.Exit(owner, isCancel: true);
        if (owner.Weapon != null)
        {
            owner.Weapon.IsInPreferredZone.Value = false;
        }
    }
}
