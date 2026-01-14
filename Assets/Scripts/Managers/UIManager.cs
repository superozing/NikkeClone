using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class UIManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.UI;

    private readonly Stack<UI_Popup> _popupStack = new();
    private Transform _sceneRoot;
    private Transform _dontDestroyRoot;
    private Camera _uiCamera;

    /// <summary>
    /// UI Popup  ο Sorting Order Դϴ.
    /// </summary>
    private int _sortingOrder = 50;

    /// <summary>
    /// Sorting Group  order 
    /// </summary>
    private const int ORDER_STEP = 10;

    public void Init()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject { name = "@EventSystem" };
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            Object.DontDestroyOnLoad(eventSystemGo);
        }

        // 초기화 시 UI 카메라 확보
        EnsureUICamera();

        // 초기 진입 시에도 Main Camera가 존재한다면 Stack에 등록합니다.
        // TestScene처럼 Awake/Start 시점에 이미 로드된 씬을 위해 필요합니다.
        if (Camera.main != null)
        {
            RegisterUICameraToStack(Camera.main);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"{ManagerType} Manager Init մϴ.");
    }

    public void Update() { }

    public void Clear()
    {
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear մϴ.");
    }

    /// <summary>
    ///  Ÿ UI_View 񵿱 εϰ,  ViewModel մϴ.
    /// </summary>
    /// <typeparam name="TView"> UI Ÿ̸, UI_View ؾ մϴ.</typeparam>
    /// <param name="viewModel">UI  ViewModel νϽԴϴ.</param>
    /// <param name="parent">UI ġ θ TransformԴϴ. null  ŸԿ  ڵ Root ˴ϴ.</param>
    /// <returns>  ʱȭ Ϸ UI νϽԴϴ.</returns>
    public async Task<TView> ShowAsync<TView>(ViewModelBase viewModel, Transform parent = null) where TView : UI_View
    {
        // θ õ  ,   UI Ʈ մϴ.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager]  ε . path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        // parent null  ÿ Pushϵ  Ȯȭմϴ.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group  մϴ.
        SetSortingGroupOrder(go, view is UI_Popup);

        // Է¹  մϴ. (⼭ AddRef)
        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    ///  Ÿ UI_View 񵿱 εϰ ȯմϴ.
    /// ResourceManagerEx  Object Pooling ڵ Ȱմϴ.
    /// </summary>
    /// <typeparam name="T"> UI Ÿ̸, UI_View ؾ մϴ.</typeparam>
    /// <param name="parent">UI ġ θ TransformԴϴ. null  ŸԿ  ڵ Root ˴ϴ.</param>
    /// <returns> UI νϽԴϴ.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        // θ õ  ,   UI Ʈ մϴ.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager]  ε . path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // parent null  ÿ Pushϵ  Ȯȭմϴ.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group  մϴ.
        SetSortingGroupOrder(go, view is UI_Popup);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    ///  ȯǾ ı ʴ UI_DontDestroyPopup 񵿱 εϰ ViewModel մϴ.
    /// </summary>
    /// <typeparam name="TView"> UI Ÿ̸, UI_DontDestroyPopup ؾ մϴ.</typeparam>
    /// <param name="viewModel">UI  ViewModel νϽԴϴ.</param>
    /// <param name="parent">UI ġ θ TransformԴϴ. null  DontDestroyRoot ⺻ ˴ϴ.</param>
    /// <returns> UI νϽԴϴ.</returns>
    public async Task<TView> ShowDontDestroyAsync<TView>(ViewModelBase viewModel) where TView : UI_DontDestroyPopup
    {
        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager]  ε . path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        SortingGroup sortingGroup = go.GetOrAddComponent<SortingGroup>();
        sortingGroup.sortingOrder = 999999;

        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    ///  ȯǾ ı ʴ UI_DontDestroyPopup 񵿱 εϰ ȯմϴ.
    ///  UI Popup Stack  , ׻ ֻܿ ǥõ˴ϴ.
    /// </summary>
    /// <typeparam name="T"> UI Ÿ̸, UI_DontDestroyPopup ؾ մϴ.</typeparam>
    /// <param name="parent">UI ġ θ TransformԴϴ. null  DontDestroyRoot ⺻ ˴ϴ.</param>
    /// <returns> UI νϽԴϴ.</returns>
    public async Task<T> ShowDontDestroyAsync<T>() where T : UI_DontDestroyPopup
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager]  ε . path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        SortingGroup sortingGroup = go.GetOrAddComponent<SortingGroup>();
        sortingGroup.sortingOrder = 999999;

        view.gameObject.SetActive(true);
        return view;
    }


    /// <summary>
    ///  UI_View ݰ Pool ȯմϴ.
    /// ˾ ,  ֻܿ     ֽϴ.
    /// </summary>
    /// <param name="view"> UI_View νϽԴϴ.</param>
    public void Close(UI_View view)
    {
        if (view == null) return;

        if (view is UI_Popup popup && view is not UI_DontDestroyPopup)
        {
            if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                _sortingOrder -= ORDER_STEP;

                //   Popup ActionMapKey 
                if (_popupStack.Count > 0)
                {
                    var nextPopup = _popupStack.Peek();
                    Managers.Input.SwitchActionMap(nextPopup.ActionMapKey);
                }
                //    ⺻ ("None")
                else
                {
                    Managers.Input.SwitchActionMap("None");
                }
            }
        }

        // UI Ǯ ȯϰų ıϱ  ViewModel   ϴ.
        //  ViewModel Release()    īƮ ϰ, ʿ OnDispose() ȣ˴ϴ.
        view.SetViewModel(null);

        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject SortingGroup Ʈ ϰ Sorting Order մϴ.
    /// </summary>
    private void SetSortingGroupOrder(GameObject go, bool useSortingOrder)
    {
        SortingGroup sortingGroup = go.GetOrAddComponent<SortingGroup>();
        if (useSortingOrder)
        {
            _sortingOrder += ORDER_STEP;
            sortingGroup.sortingOrder = _sortingOrder;
        }
        else
        {
            // sortingOrder   (UI_Popup  ) sortingOrder  
            sortingGroup.sortingOrder = 0;
        }
    }

    /// <summary>
    ///   UI Root Transform ȯմϴ.  մϴ.
    /// </summary>
    private Transform GetSceneRoot()
    {
        if (_sceneRoot == null)
        {
            GameObject rootGo = GameObject.Find("@UI_Root_Scene");

            // UI Ʈ  , Canvas ʼ Ʈ Ͽ  մϴ.
            if (rootGo == null)
            {
                rootGo = new GameObject { name = "@UI_Root_Scene" };
                rootGo.layer = LayerMask.NameToLayer("UI");

                Canvas canvas = rootGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                
                // 전용 UI 카메라 사용
                EnsureUICamera();
                canvas.worldCamera = _uiCamera; 

                CanvasScaler scaler = rootGo.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 1.0f;

                rootGo.AddComponent<GraphicRaycaster>();
            }
            _sceneRoot = rootGo.transform;
        }
        return _sceneRoot;
    }

    /// <summary>
    /// DontDestroy UI Root를 가져오거나 생성합니다.
    /// </summary>
    private Transform GetDontDestroyRoot()
    {
        if (_dontDestroyRoot != null) 
            return _dontDestroyRoot;

        GameObject dontDestroyGo = GameObject.Find("@UI_Root_DontDestroy");
        if (dontDestroyGo == null)
        {
            dontDestroyGo = new GameObject { name = "@UI_Root_DontDestroy" };
            dontDestroyGo.layer = LayerMask.NameToLayer("UI");
            Object.DontDestroyOnLoad(dontDestroyGo);
        }

        if (dontDestroyGo.GetComponent<Canvas>() == null)
        {
            Canvas canvas = dontDestroyGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            
            // 전용 UI 카메라 사용
            EnsureUICamera();
            canvas.worldCamera = _uiCamera;
            
            canvas.sortingOrder = 999999;

            CanvasScaler scaler = dontDestroyGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1.0f;

            dontDestroyGo.AddComponent<GraphicRaycaster>();
        }

        _dontDestroyRoot = dontDestroyGo.transform;
        return _dontDestroyRoot;
    }

    /// <summary>
    /// UI 전용 카메라를 확보합니다.
    /// </summary>
    private void EnsureUICamera()
    {
        if (_uiCamera != null) return;

        GameObject cameraGo = GameObject.Find("@UI_Camera");
        if (cameraGo == null)
        {
            cameraGo = new GameObject { name = "@UI_Camera" };
            Object.DontDestroyOnLoad(cameraGo);
        }

        cameraGo.layer = LayerMask.NameToLayer("UI");
        // 위치를 명시적으로 설정하여 오동작 방지
        cameraGo.transform.position = new Vector3(0, 0, -100);

        _uiCamera = cameraGo.GetOrAddComponent<Camera>();
        _uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI"); // UI 레이어만 렌더링
        _uiCamera.clearFlags = CameraClearFlags.Nothing;          // Overlay는 Clear 불필요
        _uiCamera.orthographic = false;                           // 원근 투영 활성화
        _uiCamera.fieldOfView = 60f;                              // 표준 FOV
        
        // URP Overlay Camera 설정
        var urpCameraData = _uiCamera.GetUniversalAdditionalCameraData();
        if (urpCameraData != null)
        {
            urpCameraData.renderType = CameraRenderType.Overlay;
        }
    }

    /// <summary>
    /// Main Camera의 URP Camera Stack에 UI Camera를 등록합니다.
    /// </summary>
    /// <param name="mainCamera">Base Camera로 사용할 Main Camera</param>
    private void RegisterUICameraToStack(Camera mainCamera)
    {
        if (_uiCamera == null) return;

        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (mainCameraData == null) return;
        
        // 이미 등록되어 있으면 중복 등록 방지
        if (mainCameraData.cameraStack.Contains(_uiCamera)) return;
        
        mainCameraData.cameraStack.Add(_uiCamera);
    }

    /// <summary>
    /// UI ŸԿ   θ մϴ.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// ο  ε  ȣǴ ̺Ʈ ڵ鷯Դϴ.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Camera.main != null)
        {
            // 메인 카메라가 UI 레이어를 렌더링하지 않도록 설정
            int uiLayerMask = 1 << LayerMask.NameToLayer("UI");
            Camera.main.cullingMask &= ~uiLayerMask;

            // URP Camera Stack에 UI Camera 등록
            RegisterUICameraToStack(Camera.main);
        }

        // UI 카메라는 항상 존재해야 함
        EnsureUICamera();

        Clear();
    }
}