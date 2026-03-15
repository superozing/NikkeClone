# Phase Summary: CombatVFX Phase B (데미지 넘버 시스템) & 최적화
**작성일**: 2026-03-09

## 1. 개요
이 문서는 **CombatVFX Phase B (데미지 넘버 시스템)** 세션 동안 진행된 개발 내용, 도입된 새로운 시스템, 그리고 적용된 최적화 기법을 요약한 기록입니다. 
다음 페이즈 개발자 또는 처음 코드를 보는 담당자가 전체 맥락을 한눈에 파악할 수 있도록 작성되었습니다.

---

## 2. 새로 추가/변경된 사항 (What was added & modified)

### 2.1 주요 스크립트
1. **`DamageNumberViewModel.cs`** (신규/연결)
   - CombatSystem과 UI 렌더링을 분리하는 MVVM 패턴의 중계자 역할.
   - 데미지 발생 이벤트를 뷰모델 계층에서 캡슐화하여 UI_DamageNumberSystem으로 전달합니다.
2. **`UI_DamageNumberSystem.cs`** (핵심 기능 구현)
   - 뷰모델의 이벤트를 구독하여 World 좌표를 Screen 스페이스 좌표로 변환하고 Canvas 환경에 렌더링합니다.
   - 단일 파티클 시스템을 사용하여 여러 개의 데미지 숫자를 효율적으로 방출합니다.
3. **`CombatSystem.cs`** (로직 의존성 분리)
   - 기존의 존재하지 않던 `DamageNumberSystem` 직접 결합을 제거했습니다.
   - B-1 단계에서 `DamageNumberViewModel` 인스턴스를 생성하고 피격 데이터(`HandleDamageNumber`)를 뷰모델에 통지하도록 리팩토링되었습니다.

### 2.2 리소스 및 에셋 셋업 (수동 작업 가이드화)
- **4x4 Atlas Texture**: 0~9 번호 이미지가 패킹된 텍스처 (`DamageNumber_Atlas.png`).
- **ShaderGraph (`DamageNumberDigit.shadergraph`)**: ParticleSystem의 `Custom1.x` Vertex Stream으로 전달받은 인덱스를 파싱(`col`, `row`)하여 UV를 리매핑합니다 (Custom Function HLSL 적용).
- **Material (`M_DamageNumberDigit.mat`)**: 생성된 셰이더그래프를 사용하는 머티리얼 구성.

---

## 3. 개발된 주요 기능 (What features were developed)

### 3.1 자릿수 분해 및 개별 방출 매핑 (Digit Decomposition)
- 타격 피해량(예: 1234)을 문자로 변환(`ToString` -> 수학 연산으로 이후 최적화됨)하여 각 자리 구합니다.
- 좌우 대칭을 맞추어 글자 간격(`_digitSpacing`)을 확보하고 각 자릿수에 해당하는 파티클을 가로로 정렬해 개별 `Emit`합니다.
- `CustomDataBuffer` 배열을 통해 **각 숫자의 인덱스 값**을 셰이더로 바로 넘겨줍니다.

### 3.2 캔버스 기반 렌더링 좌표계 동기화 (Canvas Positional Sync)
- 3D 월드의 타격 지점을 Canvas(정사영/Perspective 혼합 가능) 상의 `RectTransform` 로컬 좌표로 매핑합니다.
- `ParticleSystem`의 `Scaling Mode`를 `Hierarchy`로 변경하여, 거대한 거리에 스폰되는 일이 없도록 캔버스의 월드 스케일 계수(CanvasScaler)를 완벽히 상속받도록 수정했습니다.

---

## 4. 적용된 최적화 (What optimizations were applied)

> **상세 설계 문서**: `CombatVFX_Optimization_Design.md` 참고

1. **GC Allocation 완전 제거 (Zero GC)**
   - 데미지를 숫자로 쪼갤 때 발생하던 `damage.ToString()` 문자열 할당을 제거했습니다.
   - `Math.Log10`으로 총 자릿수를 알아내고, `modulo 10` 연산으로 백 단위부터 일 단위까지 숫자를 쪼개어 가비지 생성을 `0(Zero)`으로 만들었습니다.
2. **API 호출 비용 최적화 (Batching)**
   - 자릿수별로 파티클을 띄울 때 `for`문 안에서 매번 호출하던 `GetCustomParticleData` / `SetCustomParticleData`를 루프 밖으로 뺐습니다.
   - 전체 자릿수 방출(Emit) 후 **단 1회**만 데이터를 가져와 채워넣고 덮어쓰도록 수정하여 CPU 오버헤드를 크게 줄였습니다.
3. **불필요한 프레임 지연/합산 제거**
   - 샷건 등 개별 타격 수가 많은 로직에서 프레임별 버퍼를 합산해야 하는 기존 기획(Phase B-3)을 무효화했습니다.
   - API 최적화와 GC 제거가 도입되었으므로, **"피격되는 즉시 즉발 방출"** 하도록 롤백하여 구조를 훨씬 단순하고 버그 없게 유지했습니다.
4. **객체 메모리 캐싱 및 예약**
   - 매번 프로퍼티에 접근하던 `Camera.main` 호출을 `Awake`에서 프로퍼티화하여 캐시에 올렸습니다.
   - 데이터 버퍼 리스트의 컨테이너 `Capacity`를 Particle System의 `Max Particles` 수만큼 **사전 예약(Pre-warm)**하여 런타임에 리스트 크기가 동적 재할당되는 병목을 예방했습니다.

---

## 5. 결론 및 향후 계획
- Phase B의 목표였던 4x4 텍스처 아틀라스를 이용한 Canvas UI 데미지 수치 생성 시스템 구축이 안정적으로 마무리되었습니다.
- 본 세션에서 생성된 기획서, 스텝(에디터 행동 가이드), 커밋 플랜은 현재 문서와 함께 `Agent/Archive/` 폴더로 저장(이관)됩니다.
