using UnityEngine;
using UI;
using System.Collections.Generic;

/// <summary>
/// UI/ScreenSpace-Camera 캔버스 공간에서 데미지 숫자를 시각화하는 클래스입니다.
/// ViewModel로부터 데미지 정보와 월드 좌표를 전달받아 UI 파티클로 변환하여 방출합니다.
/// </summary>
public class UI_DamageNumberSystem : UI_View
{
    [Header("Dependencies")]
    [SerializeField] private ParticleSystem _particleSystem;

    [Header("Settings")]
    [SerializeField] private float _baseSize = 160f; // Canvas Pixel 단위
    [SerializeField] private float _digitSpacing = 80f; // 글자 간격

    // 숫자 -> 4x4 셀 인덱스 매핑 (Design Doc 참조)
    // 텍스처 배치: [1][2][3][4] / [5][6][7][8] / [9][0][ ][ ]
    private static readonly int[] DigitToCellIndex = { 9, 0, 1, 2, 3, 4, 5, 6, 7, 8 };

    // UIManager가 SortingGroup 정렬 시 이 값을 사용합니다.
    // ParticleSystem은 URP Transparent Queue를 통해 렌더링되므로,
    // 다른 Canvas UI(Order 0+)보다 뒤에 그려지기 위해 음수 오더를 지정합니다.
    public override int? SortingOrderOverride => -100;

    private DamageNumberViewModel _viewModel;
    private RectTransform _parentRect;

    private Camera _uiCamera;
    private Camera _mainCamera;

    protected override void Awake()
    {
        base.Awake();
        _uiCamera = Managers.UI.UICamera;
        _mainCamera = Camera.main;
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        base.SetViewModel(viewModel);
        _viewModel = viewModel as DamageNumberViewModel;

        if (_viewModel != null)
        {
            _viewModel.OnDamageEmitted += EmitDamageNumber;
        }
    }

    /// <summary>
    /// ViewModel의 요청에 따라 파티클을 생성하고 데이터를 주입합니다.
    /// [Optimization]: GC 할당(ToString) 제거 및 API Batching 적용
    /// </summary>
    /// <param name="damage">데미지 수치</param>
    /// <param name="worldPos">3D 월드 좌표</param>
    private void EmitDamageNumber(long damage, Vector3 worldPos)
    {
        if (_parentRect == null)
            _parentRect = transform.parent as RectTransform;

        if (_particleSystem == null || _uiCamera == null || _parentRect == null || _mainCamera == null)
            return;

        // 1. World -> Screen -> Canvas Local 좌표계 변환
        Vector2 screenPos = _mainCamera.WorldToScreenPoint(worldPos);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, screenPos, _uiCamera, out Vector2 localPos))
        {
            Vector3 basePos = new Vector3(localPos.x, localPos.y, 0f);

            // 2. [Optimization] 자릿수 계산 (Math.Log10 활용으로 string 할당 방지)
            int digitCount = (damage <= 0) ? 1 : (int)Mathf.Log10(damage) + 1;

            // 전체 너비 계산하여 중앙 정렬 시작점 산출
            float totalWidth = (digitCount - 1) * _digitSpacing;
            float startX = basePos.x - totalWidth * 0.5f;

            // 3. 자릿수별 파티클 방출 (우측 1의 자리부터 역순으로 계산)
            long remaining = damage;
            for (int i = digitCount - 1; i >= 0; i--)
            {
                int digit = (int)(remaining % 10);
                remaining /= 10;

                int cellIndex = DigitToCellIndex[digit];

                var emitParams = new ParticleSystem.EmitParams();
                // 왼쪽부터 배치하기 위해 X 좌표 계산: startX + (i * spacing)
                emitParams.position = new Vector3(startX + (i * _digitSpacing), basePos.y, basePos.z);
                emitParams.startSize3D = new Vector3(_baseSize, _baseSize, 1f);
                emitParams.applyShapeToPosition = false;

                // startColor의 R 채널에 셀 인덱스를 (0~1) 범위로 압축하여 저장 (분모 15)
                // 셰이더에서 VertexColor.R * 15.0 을 round() 하여 복원함
                emitParams.startColor = new Color(cellIndex / 15f, 1f, 1f, 1f);

                _particleSystem.Emit(emitParams, 1);
            }
        }
    }

    protected override void OnDestroy()
    {
        if (_viewModel != null)
        {
            _viewModel.OnDamageEmitted -= EmitDamageNumber;
        }
        base.OnDestroy();
    }
}
