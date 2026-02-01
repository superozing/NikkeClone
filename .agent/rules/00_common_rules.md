# Common Rules (공통 규칙)
모든 Role에 공통으로 적용되는 규칙입니다.

---

## 1. No Over-Abstraction for Single-Use Inline Functions
한 번만 사용되는 인라인 함수를 가독성을 위해 별도 함수로 추상화하지 않습니다.

- **원칙**: 한 번만 사용되는 코드는 inline으로 유지합니다.
- **이유**: 과도한 추상화는 코드 흐름을 분산시켜 오히려 가독성을 해칩니다.
- **예외**: 함수 이름이 복잡한 로직의 의도를 명확히 전달하거나, 테스트 용이성이 필요한 경우에만 추상화합니다.

### Example
```csharp
// ❌ Bad: 한 번만 사용되는 인라인 함수를 추상화
private void Initialize()
{
    SetupPlayer();
    ConfigureCamera();
}

private void SetupPlayer() => _player.SetActive(true);
private void ConfigureCamera() => _camera.orthographicSize = 10f;

// ✅ Good: 인라인으로 유지
private void Initialize()
{
    _player.SetActive(true);
    _camera.orthographicSize = 10f;
}
```

---

## 2. Language Convention (언어 규칙)
답변 및 문서 작성 시 다음 언어 규칙을 따릅니다.

| 항목 | 언어 |
|------|------|
| 전문 용어 (Technical Terms) | **English** |
| 일반 설명 및 문장 | **한글** |

### Examples
- ✅ `ViewModel을 생성하고 SerializeField로 바인딩합니다.`
- ✅ `Prefab 경로는 Resources/Prefabs/UI/입니다.`
- ❌ `뷰모델을 생성하고 직렬화필드로 바인딩합니다.`
- ❌ `Create the ViewModel and bind it using SerializeField.`
