# 커밋 계획: CombatVFX Phase B (데미지 수치)
**날짜**: 2026-03-09
**관련 설계 문서**: `Agent/Design/CombatVFX_PhaseB_Design.md`, `Agent/Design/CombatVFX_Optimization_Design.md`

## 요약
Phase B (데미지 수치) 구현 완료 및 성능 최적화(GC Zero, API Batching), 불필요한 SG 합산 로직 제거를 포함한 세분화된 커밋 계획입니다.

---

## 커밋 1: feat(combat): DamageNumberViewModel 추가 및 CombatSystem 통합

### 스테이징할 파일
```bash
git add Assets/Scripts/Combat/CombatSystem.cs
git add Assets/Scripts/UI/ViewModel/DamageNumberViewModel.cs
```

### 커밋 메시지
```
feat(combat): DamageNumberViewModel 추가 및 CombatSystem 통합

- MVVM 패턴 중계를 위한 DamageNumberViewModel 추가
- CombatSystem에서 뷰모델 초기화 및 적 피격 이벤트 바인딩
- 전투 시작 시 HUD 시스템 초기화 로직 구현
```

---

## 커밋 2: feat(vfx): 데미지 수치 셰이더 및 텍스처 추가

### 스테이징할 파일
```bash
git add Assets/Shaders/UI/DamageNumber.shadergraph*
git add Assets/Textures/Atlas/
git add Assets/Textures/Atlas.meta
```

### 커밋 메시지
```
feat(vfx): 데미지 수치 셰이더 및 텍스처 추가

- 4x4 아틀라스 UV 리매핑을 위한 DamageNumber.shadergraph 추가
- 숫자 아틀라스 텍스처 및 관련 메타 파일 추가
```

---

## 커밋 3: feat(ui): 최적화된 UI_DamageNumberSystem 구현

### 스테이징할 파일
```bash
git add Assets/Scripts/UI/View/UI_DamageNumberSystem.cs
```

### 커밋 메시지
```
feat(ui): 최적화된 UI_DamageNumberSystem 구현

- log10/modulo 연산을 이용한 자릿수 분해 구현 (GC 할당 제거)
- ParticleSystem CustomData 업데이트 배치 처리 구현 (CPU 오버헤드 최적화)
- 불필요한 SG 펠릿 합산 로직 제거 (즉시 방출로 변경)
- 주요 컴포넌트 캐싱 및 버퍼 용량 사전 예약 적용
```

---

## 커밋 4: feat(ui): UI_DamageNumberSystem 프리팹 및 재질 추가

### 스테이징할 파일
```bash
git add Assets/Materials/UI/Mat_DamageNumber.mat*
git add Assets/Prefabs/UI/View/UI_DamageNumberSystem.prefab*
```

### 커밋 메시지
```
feat(ui): UI_DamageNumberSystem 프리팹 및 재질 추가

- Custom Vertex Streams가 설정된 ParticleSystem 포함 프리팹 생성
- 최적화된 데미지 수치 셰이더를 사용하는 UI 재질 생성
```

---

## 커밋 5: docs(combat): Phase B 설계 및 설정 가이드 추가

### 스테이징할 파일
```bash
git add Agent/Design/CombatVFX_PhaseB_Design.md
git add Agent/Step/CombatVFX_PhaseB_Step.md
```

### 커밋 메시지
```
docs(combat): Phase B 설계 및 설정 가이드 추가

- Canvas 공간 데미지 수치 시스템 아키텍처 문서화
- ParticleSystem 및 셰이더 설정을 위한 단계별 가이드 추가
```

---

## 커밋 6: docs(combat): 구조적 최적화 설계 문서 추가

### 스테이징할 파일
```bash
git add Agent/Design/CombatVFX_Optimization_Design.md
git add Agent/Step/CombatVFX_Optimization_Step.md
```

### 커밋 메시지
```
docs(combat): 구조적 최적화 설계 문서 추가

- GC Zero 전략 및 API Batching 패턴 문서화
- 구조적 변경에 대한 기술적 근거 명시
```

---

## 검증 사항
- [x] 각 커밋 단위가 단일 논리적 변경을 대표함
- [x] 최적화 로직이 핵심 View 구현과 올바르게 그룹화됨
- [x] 에셋과 로직을 분리하여 병합 충돌 가능성을 최소화함
