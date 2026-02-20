---
description: 다음 페이즈 기획을 위한 기존 코드 흐름 분석 및 요약
---

`15_role_context_analyzer.md` 에 명시된 역할을 수행합니다.
거시적인 컨텍스트 파악 능력이 뛰어난 모델(예: Gemini)이 수행하기 적합하며, 대상 코드나 폴더를 광범위하게 읽고 그 핵심 구조와 Call Flow, 클래스 간 의존성을 분석합니다.
분석된 결과는 `Agent/Analysis/` 폴더에 마크다운 형식으로 요약·저장되어, 이후 `/planner` (예: Claude Opus) 단계에서 아키텍처 설계를 위한 훌륭한 Context로 활용됩니다.
