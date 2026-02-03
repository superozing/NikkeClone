using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UI;

public class UI_LoadingPopup : UI_DontDestroyPopup, IUIShowHideable
{
    // 로딩 팝업 입력 액션은 기본 None 처리 (Esc로 닫기 불가)
    public override string ActionMapKey => "None";

    [Header("Components")]
    [SerializeField] private Image _wipeImage;

    [Header("Settings")]
    [SerializeField] private float _wipeDuration = 0.5f;
    [SerializeField] private Ease _wipeEase = Ease.InOutQuad;

    private LoadingPopupViewModel _viewModel;
    private Material _wipeMaterial;

    // 연출 객체
    private IUIAnimation _wipeInAnim;
    private IUIAnimation _wipeOutAnim;

    protected override void Awake()
    {
        base.Awake();

        if (_wipeImage != null)
        {
            // 이미지에 Material 인스턴싱하여 원래 자산 변경 방지
            _wipeMaterial = new Material(_wipeImage.material);
            _wipeImage.material = _wipeMaterial;

            // IUIAnimation 객체 생성
            // Wipe In: 0 -> 1 (화면 열기 / 나타남) => Show Animation
            _wipeInAnim = new WipeUIAnimation(_wipeMaterial, 0f, 1f, _wipeDuration, _wipeEase, _canvasGroup);

            // Wipe Out: 1 -> 0 (화면 닫기 / 사라짐) => Hide Animation
            _wipeOutAnim = new WipeUIAnimation(_wipeMaterial, 1f, 0f, _wipeDuration, _wipeEase, _canvasGroup);
        }
    }

    void OnEnable()
    {
        // 초기 상태 설정 (Cutoff 0 -> 투명)
        if (_wipeMaterial != null)
        {
            _wipeMaterial.SetFloat(Shader.PropertyToID("_CutOff"), 0f);
        }

        // ViewModel이 할당된 경우에만 프로세스 시작
        // 오브젝트 풀 사용 시에도 OnEnable()은 항상 호출됩니다.
        if (_viewModel != null)
        {
            _viewModel.ExecuteProcess();
        }
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 구독 해제 루틴
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested -= OnWipeInRequested;
            _viewModel.OnWipeOutRequested -= OnWipeOutRequested;
            _viewModel.OnCloseRequested -= CloseSelf;
        }

        _viewModel = viewModel as LoadingPopupViewModel;

        base.SetViewModel(viewModel);

        // 이벤트 구독 등록
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested += OnWipeInRequested;
            _viewModel.OnWipeOutRequested += OnWipeOutRequested;
            _viewModel.OnCloseRequested += CloseSelf;

            // 만약 이미 활성화된 상태라면(Active Prefab 등)
            // OnEnable이 ViewModel 설정 전에 실행되었을 수 있으므로 여기서 프로세스를 시작합니다.
            if (gameObject.activeInHierarchy)
            {
                _viewModel.ExecuteProcess();
            }
        }
    }

    // --- IUIShowHideable Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_wipeInAnim != null)
            await _wipeInAnim.ExecuteAsync(delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_wipeOutAnim != null)
            await _wipeOutAnim.ExecuteAsync(delay);
    }

    // --- Event Handlers (ViewModel -> View) ---

    // Func<Task> 델리게이트 매칭을 위해 래퍼 사용
    private Task OnWipeInRequested() => PlayShowAnimationAsync();
    private Task OnWipeOutRequested() => PlayHideAnimationAsync();

    private void CloseSelf()
    {
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 생성된 Material 인스턴스 파괴 (메모리 누수 방지)
        if (_wipeMaterial != null)
        {
            Destroy(_wipeMaterial);
            _wipeMaterial = null;
        }

        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested -= OnWipeInRequested;
            _viewModel.OnWipeOutRequested -= OnWipeOutRequested;
            _viewModel.OnCloseRequested -= CloseSelf;
        }
    }
}