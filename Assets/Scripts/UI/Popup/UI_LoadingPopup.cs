using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_LoadingPopup : UI_DontDestroyPopup, IUIShowHideAnimation
{
    // �ε� �� �Է� ������ ���� None ��� (Esc�� �ݱ� �Ұ�)
    public override string ActionMapKey => "None";

    [Header("Components")]
    [SerializeField] private Image _wipeImage;

    [Header("Settings")]
    [SerializeField] private float _wipeDuration = 0.5f;
    [SerializeField] private Ease _wipeEase = Ease.InOutQuad;

    private LoadingPopupViewModel _viewModel;
    private Material _wipeMaterial;

    // ���� ��ü
    private IUIAnimation _wipeInAnim;
    private IUIAnimation _wipeOutAnim;

    protected override void Awake()
    {
        base.Awake();

        if (_wipeImage != null)
        {
            // ��Ÿ�ӿ� Material �ν��Ͻ��� �����Ͽ� ���� ���� ���� ����
            _wipeMaterial = new Material(_wipeImage.material);
            _wipeImage.material = _wipeMaterial;

            // �ʱ� ���� ���� (Cutoff 0 -> ����/����)
            _wipeMaterial.SetFloat(Shader.PropertyToID("_CutOff"), 0f);

            // IUIAnimation ����ü ����
            // Wipe In: 0 -> 1 (ȭ�� ���� / ����) => Show Animation
            _wipeInAnim = new WipeUIAnimation(_wipeMaterial, 0f, 1f, _wipeDuration, _wipeEase);

            // Wipe Out: 1 -> 0 (ȭ�� ���� / ����) => Hide Animation
            _wipeOutAnim = new WipeUIAnimation(_wipeMaterial, 1f, 0f, _wipeDuration, _wipeEase);
        }
    }

    private void Start()
    {
        // View�� Ȱ��ȭ�� ���� ViewModel�� ���μ��� ����
        _viewModel?.ExecuteProcess();
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // ���� ���� ����
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested -= OnWipeInRequested;
            _viewModel.OnWipeOutRequested -= OnWipeOutRequested;
            _viewModel.OnCloseRequested -= CloseSelf;
        }

        _viewModel = viewModel as LoadingPopupViewModel;

        base.SetViewModel(viewModel);

        // �� ���� ����
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

    // Func<Task> ��������Ʈ ���Ŀ� ���߱� ���� ����
    private Task OnWipeInRequested() => PlayShowAnimationAsync();
    private Task OnWipeOutRequested() => PlayHideAnimationAsync();

    private void CloseSelf()
    {
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // ������ Material �ν��Ͻ� �ı� (�޸� ���� ����)
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