using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_LoadingPopup : UI_DontDestroyPopup, IUIShowHideAnimation
{
    // 로딩 중 입력 차단을 위해 None 사용 (Esc로 닫기 불가)
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
            // 런타임에 Material 인스턴스를 생성하여 원본 에셋 변경 방지
            _wipeMaterial = new Material(_wipeImage.material);
            _wipeImage.material = _wipeMaterial;

            // 초기 상태 설정 (Cutoff 0 -> 투명/보임)
            _wipeMaterial.SetFloat(Shader.PropertyToID("_Cutoff"), 0f);

            // IUIAnimation 구현체 생성
            // Wipe In: 0 -> 1 (화면 덮기 / 가림) => Show Animation
            _wipeInAnim = new WipeUIAnimation(_wipeMaterial, 0f, 1f, _wipeDuration, _wipeEase);

            // Wipe Out: 1 -> 0 (화면 열기 / 보임) => Hide Animation
            _wipeOutAnim = new WipeUIAnimation(_wipeMaterial, 1f, 0f, _wipeDuration, _wipeEase);
        }
    }

    private void Start()
    {
        // View가 활성화된 직후 ViewModel의 프로세스 실행
        _viewModel?.ExecuteProcess();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 기존 구독 해제
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested -= OnWipeInRequested;
            _viewModel.OnWipeOutRequested -= OnWipeOutRequested;
            _viewModel.OnCloseRequested -= CloseSelf;
        }

        _viewModel = viewModel as LoadingPopupViewModel;

        base.SetViewModel(viewModel);

        // 새 구독 설정
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested += OnWipeInRequested;
            _viewModel.OnWipeOutRequested += OnWipeOutRequested;
            _viewModel.OnCloseRequested += CloseSelf;
        }
    }

    // --- IUIShowHideAnimation Implementation ---

    public async Task PlayShowAnimationAsync(float delay = 0f)
    {
        if (_wipeInAnim != null)
            await _wipeInAnim.ExecuteAsync(_canvasGroup, delay);
    }

    public async Task PlayHideAnimationAsync(float delay = 0f)
    {
        if (_wipeOutAnim != null)
            await _wipeOutAnim.ExecuteAsync(_canvasGroup, delay);
    }

    // --- Event Handlers (ViewModel -> View) ---

    // Func<Task> 델리게이트 형식에 맞추기 위한 래퍼
    private Task OnWipeInRequested() => PlayShowAnimationAsync();
    private Task OnWipeOutRequested() => PlayHideAnimationAsync();

    private void CloseSelf()
    {
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // 생성한 Material 인스턴스 파괴 (메모리 누수 방지)
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