# 파일 설명: 조준선(Crosshair) UI 구현
**Date**: 2026-02-22
**Purpose**: 조준선(Crosshair) UI 구현 페이즈 동안 수정되거나 추가된 모든 파일들의 세부적인 기능 명세.

---

## 1. Agent 문서 영역 (Agent Documents)
*Agent/*
역할: 기획 및 구조화 진행 내역, 작업 지침 가이드 보관
- **Analysis/*.md**: 변경 내역 분석 및 페이즈 7의 컨텍스트 정리 문서.
- **Archive/*.md**: 채택하지 않은 이전 기획안이나 과거의 기록을 보관처리한 문서 모음.
- **Design/*.md**: 신규 시스템(명시적 Crosshair 도입, 하위 클래스화, UI 적용 등)의 기술적 스펙 및 구현 지침. 프로그래머 파트를 위한 가이드.
- **Step/*.md**: 유니티 에디터상에서 Prefab, Material 등 에셋을 세팅하는 절차 가이드라인 및 워크플로우.

---

## 2. 핵심 전투 로직 & 무기 리팩토링 (Core Combat Logic & Weapon Refactoring)
*Assets/Scripts/Combat/Weapon/*, *Assets/Scripts/Combat/HFSM/*
역할: 주 무기 시스템 및 스테이트 머신의 UI 데이터 바인딩 지원
- **IWeapon.cs**, **WeaponBase.cs**, **ChargeWeaponBase.cs**: 
  - `CurrentAmmo` 및 `MaxAmmo` 등의 핵심 변수 타입을 단순 `int`에서 비동기적 UI 갱신을 위한 `ReactiveProperty<int>` 데이터 구조로 래핑하여, UI 단에서 변경 사항을 구독할 수 있도록 개편했습니다.
- **NikkeManualState.cs**, **NikkeAutoCoverState.cs**, **NikkeManualCoverState.cs**: 
  - 무기 탄창 타입 변경에 의해 발생한 기존 FSM 상태의 컴파일 에러 수정입니다. 탄창을 비교하는 조건문에서 `ReactiveProperty` 객체 대신 해당 타입의 `.Value` 필드를 참조하여 원시 값을 정확하게 비교하도록 로직이 보완되었습니다.

---

## 3. 조준선 UI 단위 로직 (Crosshair UI Logic - Views & ViewModels)
*Assets/Scripts/UI/View/Crosshair/*, *Assets/Scripts/UI/ViewModel/*
역할: 십자선 UI 시스템의 독립적인 뷰(View)와 모델(ViewModel) 레이어 구현
- **CrosshairViewModel.cs**: 
  - 조준선의 순수한 상태 데이터(Target Position/Alpha, 현재 장탄 수, 최대 장탄 수, 차지 게이지 량 등)를 유지하고 있으며, View에서 이를 구독하고 반응하여 UI 요소를 다시 그리게 돕는 브릿지 역할을 수행합니다.
- **UI_CrosshairBase.cs**: 
  - 십자선 UI를 담당하는 최상위 추상 클래스. Screen Camera 공간에서 UI의 Canvas Space까지 넘어오는 과정 중 발생하는 Perspective Projection의 영향을 역보정합니다. 조준선이 실제 월드 타겟팅이나 마우스의 화면 궤도의 오차 없이 정확히 포지셔닝될 수 있도록 좌표 다항을 연산 및 바인딩 시키는 핵심 컴포넌트입니다.
- **UI_DefaultCrosshair.cs**: 
  - 특별한 차징 게이지 등 부가 기믹 없이 즉발로 발사되는 일반 총기(AR, MG, SG, SMG 등)가 공통적으로 쓰는 UI 조준선의 베이스 모델 클래스입니다.
- **UI_ChargeCrosshair.cs**: 
  - SR, RL 등 차징 이후 발사되는 무기가 사용하는 십자선 컨트롤러입니다. 내부 차지 게이지 Image UI 갱신 등 추가적인 로직을 포함합니다.
- **Type/UI_[WeaponType]Crosshair.cs**: 
  - 각 무기별 십자선의 최종 말단 자식 스크립트(예: `UI_ARCrosshair`, `UI_SRCrosshair`). Inspector 창에서 각 무기별 성향에 따른 추가 애니메이션이나 고유 설정을 부여할 수 있는 Type Identifier 식별자 용도를 가집니다.

---

## 4. 전투 시스템 통합 (Combat Integration Systems)
*Assets/Scripts/Combat/CombatCrosshairSystem.cs*, *Assets/Scripts/Combat/CombatSystem.cs*, *Assets/Scripts/UI/View/UI_CombatHUD.cs*
역할: 전투 화면 내 조준선 시스템의 런타임 생성, 제어 및 상호작용 브릿지
- **CombatCrosshairSystem.cs**: 
  - 전투 도중 현재 픽된 니케의 무기에 대응하는 십자선 프리팹을 동적으로 로드(어드레서블/리소스)하고 해제합니다. Lifecycle Controller 기능을 수행하며 현재 조작 중인 커서에 맞게 조준선 좌표를 지속해서 업데이트 및 주입합니다.
- **CombatSystem.cs**: 
  - 전투 씬의 메인 초기화 과정 내부 파이프라인에 `CombatCrosshairSystem`을 자식 시스템으로 추가 및 등록하여 프레임별 상태 동기화가 이루어지도록 변경되었습니다.
- **CombatNikke.cs**: 
  - 유저 조작의 대상이었던 컴뱃 니케가 다른 대상으로 교체될 때(Switching) 이전 무기 관리를 끊어내고, 새로운 무기의 데이터 및 Crosshair 타겟 이벤트를 적재적소 HUD로 전환하는 명령을 보냅니다.
- **UI_CombatHUD.cs**, **CombatHUDViewModel.cs**: 
  - 화면 전반의 메인 전투 HUD 요소 중 데이터와 뷰를 관장하는 부분. CrosshairViewModel이 주입되었을 때 하위 계층에 전달하거나 데이터를 보여줄 수 있도록 인터페이스가 보완되었습니다.

---

## 5. 에셋, 프리팹 및 에디터 셋팅 (Assets, Prefabs, and Editor Settings)
*Assets/Prefabs/UI/View/Crosshair/*, *Assets/Scenes/*, *Assets/Textures/*, *AddressableAssetsData/*
역할: 시각적 표현 에셋의 정립, 데이터베이스에 리소스 등록 보존
- **UI_[WeaponType]Crosshair.prefab**: 
  - 무기별 고유한 애니메이션, 이미지 컨테이너, Layout Group 등이 세팅된 최종 완성본 조준선 게임오브젝트 프리팹 파일.
- **AddressableAssetSettings.asset**, **Default Local Group.asset**: 
  - 분리 및 독립화된 십자선 프리팹 에셋들을 런타임 코드상에서 동적 로드하기 위해 어드레서블 목록(Addressable Directory)에 추가하고 변경된 에디터 세팅을 저장한 부분입니다.
- **CombatScene.unity**: 
  - `CombatCrosshairSystem` 컴포넌트를 기존 Manager에 직접 할당하거나, UI Camera/Canvas의 Render Mode 설정 등 씬 레벨 계층에 가해진 환경 세팅 변경 사항의 보존 파일.
- **AmmoBg.png**, **charge_gauge.png**: 
  - 십자선 UI 제작에 사용된 배럴/총알/차지 배경 텍스쳐. 기존 Cover 폴더에 있던 것을 올바른 Crosshair 디렉토리로 이관하였으며, 차지 게이지 신규 리소스가 추가되었습니다.
- **UIManager.cs**, **UI_CombatResultVictoryPopup.cs**:
  - 프로젝트 공통 매니저 또는 별도의 팝업 컨테이너 창에 있던 사소한 포맷팅, 텍스트 줄바꿈 로직 오류 등의 유지보수(Fix) 건.
- **TextMesh Pro / Fonts & Materials/*.asset**:
  - 프로젝트 내에 존재하는 TMP 에셋들의 직렬화, 또는 GUID 참조 캐싱 등에 의해 에디터 상에서 자동으로 버전 관리 내역에 올라온 메타성 변경 파일. (코드 및 구조적 논리 등은 수정 없음)
