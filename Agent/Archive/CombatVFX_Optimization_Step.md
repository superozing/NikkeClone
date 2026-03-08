# Asset Workflow: CombatVFX Phase B Optimization
**Target**: `Agent/Step/CombatVFX_Optimization_Step.md`
**Audience**: User / Technical Artist (Manual Editor Work)

---

## 1. Overview

이번 작업(최적화)은 기존 C# 스크립트(`UI_DamageNumberSystem.cs`)의 메모리 할당(GC) 및 파티클 시스템 API 호출을 최적화하는 데 중점을 둡니다.

> [!NOTE]
> **Editor 내 에셋 변경 사항은 없습니다.**
> - 기존 `UI_DamageNumberSystem.prefab` 유지
> - ShaderGraph / Material 유지
> - 텍스처 아틀라스 유지

따라서 이번 패스에서는 Unity Editor 상에서의 수동 설정 단계가 생략되며, `프로그래머`가 코드를 바로 수정(Impl)하면 됩니다.
