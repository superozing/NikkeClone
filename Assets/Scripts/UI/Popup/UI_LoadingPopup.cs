using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_LoadingPopup : UI_DontDestroyPopup, IUIShowHideAnimation
{
    // 濡쒕뵫 ?앹뾽 ?낅젰 ?≪뀡? 湲곕낯 None 泥섎━ (Esc濡??リ린 遺덇?)
    public override string ActionMapKey => "None";

    [Header("Components")]
    [SerializeField] private Image _wipeImage;

    [Header("Settings")]
    [SerializeField] private float _wipeDuration = 0.5f;
    [SerializeField] private Ease _wipeEase = Ease.InOutQuad;

    private LoadingPopupViewModel _viewModel;
    private Material _wipeMaterial;

    // ?곗텧 媛앹껜
    private IUIAnimation _wipeInAnim;
    private IUIAnimation _wipeOutAnim;

    protected override void Awake()
    {
        base.Awake();

        if (_wipeImage != null)
        {
            // ?고??꾩뿉 Material ?몄뒪?댁떛?섏뿬 ?먮옒 ?먯궛 蹂寃?諛⑹?
            _wipeMaterial = new Material(_wipeImage.material);
            _wipeImage.material = _wipeMaterial;

            // IUIAnimation 媛앹껜 ?앹꽦
            // Wipe In: 0 -> 1 (?붾㈃ ??린 / ?ロ옒) => Show Animation
            _wipeInAnim = new WipeUIAnimation(_wipeMaterial, 0f, 1f, _wipeDuration, _wipeEase);

            // Wipe Out: 1 -> 0 (?붾㈃ ?닿린 / ?대┝) => Hide Animation
            _wipeOutAnim = new WipeUIAnimation(_wipeMaterial, 1f, 0f, _wipeDuration, _wipeEase);
        }
    }

    void OnEnable()
    {
        // 珥덇린 ?곹깭 ?ㅼ젙 (Cutoff 0 -> ?щ챸/?대┝)
        if (_wipeMaterial != null)
        {
            _wipeMaterial.SetFloat(Shader.PropertyToID("_CutOff"), 0f);
        }

        // ViewModel???좊떦??寃쎌슦?먮쭔 ?꾨줈?몄뒪 ?쒖옉
        // ?留??ъ궗???쒖뿉??OnEnable()? ??긽 ?몄텧?⑸땲??
        if (_viewModel != null)
        {
            _viewModel.ExecuteProcess();
        }
    }

    public override void SetViewModel(ViewModelBase viewModel)
    {
        // 援щ룆 ?댁젣 猷⑦떞
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested -= OnWipeInRequested;
            _viewModel.OnWipeOutRequested -= OnWipeOutRequested;
            _viewModel.OnCloseRequested -= CloseSelf;
        }

        _viewModel = viewModel as LoadingPopupViewModel;

        base.SetViewModel(viewModel);

        // ??援щ룆 ?깅줉
        if (_viewModel != null)
        {
            _viewModel.OnWipeInRequested += OnWipeInRequested;
            _viewModel.OnWipeOutRequested += OnWipeOutRequested;
            _viewModel.OnCloseRequested += CloseSelf;

            // 留뚯빟 ?대? ?쒖꽦?붾맂 ?곹깭?쇰㈃(Active Prefab ??
            // OnEnable??ViewModel ?ㅼ젙 ?꾩뿉 ?ㅽ뻾?섏뿀?????덉쑝誘濡??ш린???꾨줈?몄뒪瑜??쒖옉?⑸땲??
            if (gameObject.activeInHierarchy)
            {
                _viewModel.ExecuteProcess();
            }
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

    // Func<Task> ?몃━寃뚯씠??留ㅼ묶???꾪빐 ?섑띁 ?ъ슜
    private Task OnWipeInRequested() => PlayShowAnimationAsync();
    private Task OnWipeOutRequested() => PlayHideAnimationAsync();

    private void CloseSelf()
    {
        Managers.UI.Close(this);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // ?앹꽦??Material ?몄뒪?댁뒪 ?뚭눼 (硫붾え由??꾩닔 諛⑹?)
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