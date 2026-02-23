using UnityEngine;
using UnityEngine.InputSystem;

public class NikkeManualAttackState : IState<CombatNikke>
{
    public void Enter(CombatNikke owner)
    {
        if (owner.IsDead || owner.Weapon == null || !owner.Weapon.CanFire) return; // 방어 코드

        owner.View.UpdateVisualState(eNikkeState.Attack);

        // 무기 로직 시작 (발사음, 이펙트 준비 등)
        owner.Weapon?.Enter(owner);
    }

    public void Execute(CombatNikke owner)
    {
        if (owner.IsDead || owner.Weapon == null || !owner.Weapon.CanFire)
        {
            // 탄약 다 떨어지면 Reload로 전환? 
            // 이는 상위(ManualState)에서 처리하거나 여기서 요청.
            // 하지만 SubState는 상위 StateMachine을 모름.
            // Owner가 Reload를 요청하도록 해야 함.
            // owner.Reload(); // REMOVED: Managed by ManualState polling 
            // 아니, ManualState가 Update에서 탄약을 체크하고 있음.
            // 따라서 여기는 로직만 수행하면 됨.
            return;
        }

        Vector2 screenPos = owner.Weapon?.CurrentAimScreenPosition.Value ?? Vector2.zero;

        // 카메라에서 화면 클릭 지점을 향해 Ray 발사
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        Vector3 targetWorldPos;

        // 적군이나 지형 등 지정된 레이어(LayerMask) 충돌 검사
        // WeaponBase에서 사용하는 마스크와 동일하게 설정
        int layerMask = LayerMask.GetMask("CombatRapture", "CombatObstacle");

        bool isAdvantage = false;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
        {
            targetWorldPos = hit.point;

            // [추가] 피격된 적이 타겟 구역에 있는지 계산 (수동 조준 피드백용)
            // 성능이 중요하다면 Raycast 대신 화면 중앙 타겟팅 로직을 추가할 수도 있음.
            var rapture = hit.collider.GetComponent<CombatRapture>();
            if (rapture != null && owner.Weapon != null)
            {
                isAdvantage = owner.Weapon.IsPreferredZone(rapture.CurrentZone);
            }
        }
        else
        {
            // 허공을 쐈을 경우, 충분히 먼 거리를 목표 지점으로 설정
            targetWorldPos = ray.GetPoint(100f);
        }

        if (owner.Weapon != null)
        {
            owner.Weapon.Update(owner, targetWorldPos);
            owner.Weapon.IsInPreferredZone.Value = isAdvantage;
        }
    }

    public void Exit(CombatNikke owner)
    {
        // 상태 전이에 의한 중단인지, 버튼 해제인지 구분 필요.
        // 현재는 상위 상태에서 FireCanceled에 의해 Exit되는 경우이므로 isCancel=false. 
        // 조작 니케 전환 등으로 강제로 Exit 시키는 경우엔 상위 State 처리가 필요하지만 일단 Manual에서의 Exit은 버튼 뗌으로 간주.
        owner.Weapon?.Exit(owner, isCancel: false);
        if (owner.Weapon != null)
        {
            owner.Weapon.IsInPreferredZone.Value = false;
        }
    }
}
