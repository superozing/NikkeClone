# 커밋 플랜: 조준선(Crosshair) UI 구현
**Date**: 2026-02-22
**Related Design**: `Agent/Design/Phase7_1_CrosshairUI_Refactor_Design.md`, `Agent/Design/CombatCrosshairSystemRefine_Design.md`

## 요약 (Summary)
이 커밋 플랜은 신규 무기별 조준선(Crosshair) UI 시스템 구현에 대한 내용입니다. 기본 무기 데이터 속성을 `ReactiveProperty`로 리팩토링하는 작업부터, 생명 주기 및 좌표 변환을 관리하는 `CombatCrosshairSystem` 추가, 각 무기 유형별 구체적인 UI 뷰 및 뷰모델 구현이 포함됩니다.

---

## Commit 1: docs(agent): 페이즈 7 조준선 UI 기획 및 분석 문서 추가

### Files to Stage
```bash
git add Agent/Analysis/2026-02-20_Phase7_Analysis.md
git add Agent/Analysis/Phase7_Context_Analysis.md
git add Agent/Archive/2026-02-20_WeaponHandler_CommitPlan.md
git add Agent/Archive/Phase7_WeaponHandler_Design.md
git add Agent/Archive/Phase7_WeaponHandler_Step.md
git add Agent/Design/CombatCrosshairSystemRefine_Design.md
git add Agent/Design/CombatScene_Design_v2.md
git add Agent/Design/Phase5_NikkeSwitching_Design.md
git add Agent/Design/Phase6_1_CombatSceneRefactoring_Design.md
git add Agent/Design/Phase6_CombatResult_Design.md
git add Agent/Design/Phase7_1_CrosshairUI_Design.md
git add Agent/Design/Phase7_1_CrosshairUI_Refactor_Design.md
git add Agent/Step/CombatCrosshairSystemRefine_Step.md
git add Agent/Step/Phase7_1_CrosshairUI_Refactor_Step.md
git add Agent/Step/Phase7_1_CrosshairUI_Step.md
```

### Commit Message
```
docs(agent): 페이즈 7 조준선 UI 기획 및 분석 문서 추가

- 신규 Crosshair UI 시스템 및 개선을 위한 기획/작업 단계(Step) 문서 추가
- 이전 페이즈 7 무기 핸들러 기획안 등 과거 기록 분석 및 아카이빙 처리
```

### Rationale
Agent 관련 기획 및 분석 문서를 먼저 커밋하여, 프로덕션 코드와 섞이지 않게 변경 사항의 청사진을 제시합니다.

---

## Commit 2: refactor(weapon): UI 바인딩을 위한 탄약 속성을 ReactiveProperty로 업데이트

### Files to Stage
```bash
git add Assets/Scripts/Combat/Weapon/IWeapon.cs
git add Assets/Scripts/Combat/Weapon/WeaponBase.cs
git add Assets/Scripts/Combat/Weapon/ChargeWeaponBase.cs
git add Assets/Scripts/Combat/HFSM/NikkeManualState.cs
git add Assets/Scripts/Combat/HFSM/SubStates/NikkeAutoCoverState.cs
git add Assets/Scripts/Combat/HFSM/SubStates/NikkeManualCoverState.cs
```

### Commit Message
```
refactor(weapon): UI 바인딩을 위한 탄약 속성을 ReactiveProperty로 업데이트

- CurrentAmmo 및 MaxAmmo를 기본 int 타입에서 ReactiveProperty<int>로 전환
- HFSM 상태(NikkeManualState, NikkeAutoCoverState 등)에서 탄약 비교 시 직접 비교 대신 .Value 속성을 사용하도록 컴파일 에러 수정
```

### Rationale
UI 바인딩 로직을 도입하기 전에 필요한 반응형 데이터 기반을 확립합니다. 기본 인터페이스 수정에 따른 스테이트 머신의 컴파일 에러 수정도 포함됩니다.

---

## Commit 3: feat(crosshair): 무기별 조준선 UI 및 뷰모델 구현

### Files to Stage
```bash
git add Assets/Scripts/UI/ViewModel/CrosshairViewModel.cs
git add Assets/Scripts/UI/View/Crosshair.meta
git add Assets/Scripts/UI/View/Crosshair/UI_CrosshairBase.cs
git add Assets/Scripts/UI/View/Crosshair/UI_CrosshairBase.cs.meta
git add Assets/Scripts/UI/View/Crosshair/UI_DefaultCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/UI_DefaultCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/UI_ChargeCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/UI_ChargeCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_ARCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_ARCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_MGCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_MGCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_RLCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_RLCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SGCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SGCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SMGCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SMGCrosshair.cs.meta
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SRCrosshair.cs
git add Assets/Scripts/UI/View/Crosshair/Type/UI_SRCrosshair.cs.meta
```

### Commit Message
```
feat(crosshair): 무기별 조준선 UI 및 뷰모델 구현

- 조준선 UI 상태(타겟 알파, 탄약, 차지, 위치) 관리를 위한 CrosshairViewModel 추가
- 3D 캔버스 공간과 화면 간의 원근(Perspective) 좌표 매핑을 처리하는 UI_CrosshairBase 구현
- 기본 조준선(UI_DefaultCrosshair) 및 차지 조준선(UI_ChargeCrosshair) 뷰 구현
- 모든 무기 유형(AR, MG, RL, SG, SMG, SR)에 대한 고유 하위 뷰 컴포넌트 추가
```

### Rationale
UI 로직, ViewModel 및 까다로운 좌표 계산을 단일 View 컴포넌트에 분리 캡슐화합니다. 앞선 커밋에서 만든 ReactiveProperty 데이터 계층에 의존합니다.

---

## Commit 4: feat(combat): 전투 씬에 CombatCrosshairSystem 통합

### Files to Stage
```bash
git add Assets/Scripts/Combat/CombatCrosshairSystem.cs
git add Assets/Scripts/Combat/CombatCrosshairSystem.cs.meta
git add Assets/Scripts/Combat/CombatSystem.cs
git add Assets/Scripts/Combat/Entity/CombatNikke.cs
git add Assets/Scripts/UI/View/UI_CombatHUD.cs
git add Assets/Scripts/UI/ViewModel/CombatHUDViewModel.cs
```

### Commit Message
```
feat(combat): 전투 씬에 CombatCrosshairSystem 통합

- 활성화된 조준선의 생성, 생명주기 및 좌표 업데이트를 관리하는 CombatCrosshairSystem 추가
- CombatSystem의 루프를 통해 CombatCrosshairSystem 초기화 및 구동
- CombatNikke에서 다른 니케로 조작을 전환할 때 무기 데이터와 이벤트를 HUD로 전달하도록 연결
- 주입된 조준선을 수용하기 위해 UI_CombatHUD 및 CombatHUDViewModel 수정 연동
```

### Rationale
분리된 UI 뷰(Commit 3)를 전체 전투 시스템(CombatSystem 및 CombatNikke)의 생명주기에 결합시킵니다. 데이터가 적절한 위치에서 갱신 및 주입되도록 시스템을 연동합니다.

---

## Commit 5: chore(asset): 조준선 프리팹, 텍스처 및 어드레서블 설정 추가

### Files to Stage
```bash
git add Assets/Prefabs/UI/View/Crosshair.meta
git add Assets/Prefabs/UI/View/Crosshair/UI_ARCrosshair.prefab*
git add Assets/Prefabs/UI/View/Crosshair/UI_MGCrosshair.prefab*
git add Assets/Prefabs/UI/View/Crosshair/UI_RLCrosshair.prefab*
git add Assets/Prefabs/UI/View/Crosshair/UI_SGCrosshair.prefab*
git add Assets/Prefabs/UI/View/Crosshair/UI_SMGCrosshair.prefab*
git add Assets/Prefabs/UI/View/Crosshair/UI_SRCrosshair.prefab*
git add Assets/Prefabs/UI/View/UI_SRCrosshair.prefab*
git add Assets/Scenes/CombatScene.unity
git add Assets/AddressableAssetsData/AddressableAssetSettings.asset
git add "Assets/AddressableAssetsData/AssetGroups/Default Local Group.asset"
git add Assets/Textures/Combat/Cover/AmmoBg.png*
git add Assets/Textures/Combat/Crosshair/AmmoBg.png*
git add Assets/Textures/Combat/Crosshair/charge_gauge.png*
git add Assets/Textures/Icon/white.png.meta
git add "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/"*.asset
git add Assets/Scripts/Managers/UIManager.cs
git add Assets/Scripts/UI/Popup/UI_CombatResultVictoryPopup.cs
```
*(참고: 와일드카드 명령어로 추가할 때 매칭되지 않는 메타 파일 등에 유의하여 에디터 상에서 작업하시길 권장합니다)*

### Commit Message
```
chore(asset): 조준선 프리팹, 텍스처 및 어드레서블 설정 추가

- 무기 종류별 다양한 조준선 구조를 가진 Unity 프리팹 추가
- AmmoBg.png를 조준선 디렉토리로 이동 및 charge_gauge.png 추가
- CombatScene.unity 내의 매니저 및 참조 재설정
- 새로운 UI 에셋을 추적하도록 AddressableAssetSettings 및 Default Local Group 업데이트
- UIManager 띄어쓰기 및 UI_CombatResultVictoryPopup의 사소한 로직 정리
- TMP 폰트 직렬화 노이즈 정리
```

### Rationale
생성된 프리팹(.prefab), 씬 파일(.unity), 에셋 포인터, 이미지 텍스처 등 에디터 상에서 수정된 바이너리/데이터 성격의 파일들을 마지막에 추가합니다.

---

## 실행 순서 (Execution Order)
1. 변경 사항의 청사진인 문서(Agent Docs)를 먼저 가장 선행 구조로 커밋합니다. (Commit 1)
2. UI 적용을 위해 기반 데이터가 되는 무기 리팩토링 내용을 커밋합니다. (Commit 2)
3. UI 시스템(뷰모델과 뷰) 자체를 분리해 추가합니다. (Commit 3)
4. 그 UI 시스템을 메인 전투 시스템과 연결합니다. (Commit 4)
5. 앞선 스크립트 수정 사항이 실제 반영된 에디터 데이터 및 프리팹 에셋을 최종 묶어서 커밋합니다. (Commit 5)

## 검증 사항 (Verification)
- [x] 각 커밋이 독립적으로 컴파일 무결성을 보장하는가?
- [x] 커밋 메시지가 컨벤션(Type, Scope, Subject)을 준수하는가?
- [x] 논리적으로 관계없는 변경사항이 한 커밋에 섞이지 않았는가?
