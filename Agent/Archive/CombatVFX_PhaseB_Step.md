# Asset Workflow: CombatVFX Phase B (Damage Numbers) v2
**Target**: `Agent/Step/CombatVFX_PhaseB_Step.md`
**Audience**: User / Technical Artist (Manual Editor Work)
**작성일**: 2026-03-08

---

## 1. Texture Setup

### 1.1 Number Atlas 텍스처

- **경로**: `Assets/Resources/Textures/VFX/DamageNumber_Atlas.png`
- **텍스처 구조**: 사용자 보유 4×4 텍스처 (1234 / 5678 / 90□□)
- **Import Settings**:

| 파라미터 | 값 | 비고 |
|----------|-----|------|
| Texture Type | `Default` | Sprite 아님 — Shader에서 직접 샘플링 |
| Alpha Source | `Input Texture Alpha` | 또는 `From Gray Scale` |
| Alpha Is Transparency | ✅ 체크 | |
| Filter Mode | `Bilinear` | |
| Wrap Mode | `Clamp` | |

---

## 2. ShaderGraph 생성

**경로**: `Assets/Shaders/VFX/DamageNumberDigit.shadergraph`

### 2.1 생성 절차
1. Project 창 우클릭 → `Create` → `Shader Graph` → `URP` → `Unlit Shader Graph`.
2. 이름을 `DamageNumberDigit`으로 변경.
3. 더블클릭으로 ShaderGraph 에디터를 엽니다.

### 2.2 Graph Settings (Graph Inspector)
- **Surface Type**: `Transparent`
- **Blending Mode**: `Alpha`
- **Render Face**: `Both`

### 2.3 Properties (Blackboard)

| Property 이름 | Type | Default | 설명 |
|---------------|------|---------|------|
| `_MainTex` | Texture2D | (DamageNumber_Atlas) | 숫자 아틀라스 텍스처 |

### 2.4 커스텀 HLSL 노드 구성 (추천 방식)

복잡한 사칙연산 노드를 일일이 연결하는 대신, **Custom Function** 노드를 사용하여 한 번에 처리하는 것이 훨씬 직관적입니다.

1. **Custom Function 노드 추가**:
   - ShaderGraph 바탕 우클릭 → `Create Node` → `Custom Function`.
2. **노드 설정 (Graph Inspector)**:
   - **Type**: `String`
   - **Name**: `GetDigitUV`
   - **Inputs**:
     - `CellIndex` (Float)
     - `UV` (Vector 2)
   - **Outputs**:
     - `OutUV` (Vector 2)
   - **Body**: (아래 코드 복사/붙여넣기)
     ```hlsl
     // 4x4 아틀라스 (총 16칸) 기준 계산
     float col = fmod(CellIndex, 4.0);
     float row = floor(CellIndex / 4.0);
     
     // 각 셀의 폭과 높이는 0.25 (1/4)
     float uMin = col * 0.25;
     
     // V축은 Unity 텍스처 좌표계(아래가 0)에 맞게 뒤집어 줌
     float vMin = 1.0 - (row + 1.0) * 0.25;
     
     OutUV = float2(uMin + UV.x * 0.25, vMin + UV.y * 0.25);
     ```

### 2.5 그래프 최종 연결

이제 위에서 만든 `GetDigitUV` 노드를 활용해 전체 그래프를 아주 간결하게 연결합니다.

1. **UV 변환 연결**:
   - `UV` 노드 하나만 생성 (채널: **UV0**) → 출력을 `Split` 노드에 연결합니다.
   - `Split` 노드의 **B(z)** 채널 출력을 → `GetDigitUV` 노드의 **CellIndex** 입력에 연결.
   - `UV` 노드의 원래 출력을 → `GetDigitUV` 노드의 **UV** 입력에 다시 연결해도 되지만, 깔끔하게 하려면 `Split`의 R(x), G(y)를 합친 `Combine` 노드(혹은 원본 UV 자체)를 넘겨주면 됩니다. (원본 UV를 바로 연결해도 Custom Function에서 UV.xy만 취급하므로 무방합니다.)
2. **텍스처 샘플링 연결**:
   - `GetDigitUV` 노드의 **OutUV** 출력을 → `Sample Texture 2D` 노드의 **UV** 입력에 연결.
   - `Sample Texture 2D`의 Texture에는 Blackboard에 만든 `_MainTex` 프로퍼티 연결.
   - `Sample Texture 2D`의 **RGBA(또는 RGB)** 출력을 → 마스터 노드(Fragment)의 **Base Color**에 연결.
3. **알파 제어 (퇴장 페이드)**:
   - `Vertex Color` 노드 생성.
   - `Multiply` 노드 생성 → A에는 `Sample Texture 2D`의 **A(Alpha)**, B에는 `Vertex Color`의 **A(Alpha)** 연결.
   - `Multiply`의 출력을 → 마스터 노드(Fragment)의 **Alpha**에 연결.

**전체 노드 흐름 요약:**
```text
[UV0] ────┬─> [Split] ─(B)─> [GetDigitUV] ──> [Sample Texture] ──> BaseColor
          │                    ▲                         │
          └────────────────────┘                         [Alpha] × [Vertex Color_A] ──> Alpha
```
---

## 3. Material Setup

- **경로**: `Assets/Resources/Materials/VFX/M_DamageNumberDigit.mat`
- **Shader**: `Shader Graphs/DamageNumberDigit` 선택.
- **_MainTex**: `DamageNumber_Atlas` 텍스처 할당.

---

## 4. Prefab Setup

### 4.1 UI_DamageNumberSystem 프리팹

**경로**: `Assets/Prefabs/UI/View/UI_DamageNumberSystem.prefab`

**Hierarchy**:
```text
UI_DamageNumberSystem (RectTransform)
└── DamageParticle (ParticleSystem)
```

**Components (Root)**:
- `UI_DamageNumberSystem.cs`
- `RectTransform`: Anchor Stretch-Stretch (전체 화면)

**Components (DamageParticle)**:
- `ParticleSystem` (설정은 Section 5 참조)

> [!NOTE]
> `UI_DamageNumberSystem`은 `Managers.UI.ShowAsync<UI_DamageNumberSystem>(viewModel)` 으로 런타임에 생성되므로, 프리팹 이름이 클래스명과 일치해야 합니다.

---

## 5. Particle System 세부 파라미터

### Main Module

| 파라미터 | 값 | 비고 |
|----------|-----|------|
| Duration | 1.00 | |
| Looping | Off | |
| Prewarm | Off | |
| Start Delay | 0 | |
| Start Lifetime | 1.2 | 기획 조절 |
| Start Speed | 0 | |
| Start Size | 1.0 | Canvas pixel 기준 |
| Gravity Modifier | 0 | |
| Simulation Space | Local | Canvas 하위이므로 Local |
| Scaling Mode | **Hierarchy** | Canvas의 월드 스케일을 상속받아야 Canvas 픽셀 좌표가 올바르게 적용됨 |
| Play On Awake | Off | |
| Max Particles | 100 | 동시 표시 가능 최대 파티클 |

### Emission
- Rate over Time: 0
- Bursts: 없음 (스크립트 Emit)

### Shape
- **모듈 비활성화**

### Velocity over Lifetime
- 모듈 활성화
- Linear Y: `80 ~ 120` (픽셀 단위 위로 이동)

### Color over Lifetime
- 모듈 활성화
- Alpha 커브: 0~0.7 구간 Alpha 1.0 유지 → 0.7~1.0 구간 Alpha 0으로 페이드아웃

### Size over Lifetime
- 모듈 활성화
- 별도 X/Y 커브:
  - X: 0~0.8 기본(1.0) → 0.8~1.0 **0.8**
  - Y: 0~0.8 기본(1.0) → 0.8~1.0 **0.0** (수직 축소)

### Texture Sheet Animation
- **모듈 비활성화** (Shader가 UV 제어)

### Renderer

| 파라미터 | 값 |
|----------|-----|
| Render Mode | Billboard |
| Material | `M_DamageNumberDigit` |
| **Custom Vertex Streams** | 아래 참조 |

### Custom Vertex Streams 설정 (핵심)

`Custom Vertex Streams` 체크박스 **활성화** 후 다음 순서로 추가:

| # | Stream | ShaderGraph 채널 | 용도 |
|---|--------|------------------|------|
| 1 | Position (POSITION.xyz) | POSITION | 버텍스 위치 |
| 2 | Color (COLOR.xyzw) | COLOR | Vertex Color (Alpha 페이드) |
| 3 | UV (TEXCOORD0.xy) | UV0.xy (R, G 채널) | 쿼드 UV (0~1) |
| 4 | Custom1.x (TEXCOORD0.z) | UV0.z (B 채널) | 4x4 아틀라스 셀 인덱스 (0~15) |

> [!NOTE]
> Unity Particle System은 메모리 최적화를 위해 여러 스트림을 하나로 압축(Pack)합니다. 사진에서 보시듯 `UV`와 `Custom1.x`가 모두 `TEXCOORD0`의 파트로 묶여서 전달됩니다!
> 즉, `UV0`의 **x, y (또는 R, G) 채널**이 이미지 UV이고, **z (또는 B) 채널**이 셀 인덱스 상수값입니다.

---

## 6. Integration

1. `UI_DamageNumberSystem.prefab`은 `Assets/Prefabs/UI/View/` 에 배치합니다.
2. 런타임에 `CombatSystem.InitializeHUDAsync()`에서 자동 생성됩니다.
3. Inspector 할당:
   - `UI_DamageNumberSystem._particleSystem` → 하위 `DamageParticle` 오브젝트의 ParticleSystem 연결.
