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
    /// UI Popup  恝 Sorting Order 都求.
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

        // 珥덇린????UI 移대찓???뺣낫
        EnsureUICamera();

        // 珥덇린 吏꾩엯 ?쒖뿉??Main Camera媛 議댁옱?쒕떎硫?Stack???깅줉?⑸땲??
        // TestScene泥섎읆 Awake/Start ?쒖젏???대? 濡쒕뱶???ъ쓣 ?꾪빐 ?꾩슂?⑸땲??
        if (Camera.main != null)
        {
            RegisterUICameraToStack(Camera.main);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"{ManagerType} Manager Init 爛求.");
    }

    public void Update() { }

    public void Clear()
    {
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear 爛求.");
    }

    /// <summary>
    ///  타 UI_View 宙엽 琯構,  ViewModel 爛求.
    /// </summary>
    /// <typeparam name="TView"> UI 타見, UI_View 瞞 爛求.</typeparam>
    /// <param name="viewModel">UI  ViewModel 館絿都求.</param>
    /// <param name="parent">UI 치 罐 Transform都求. null  타篤  湄 Root 絳求.</param>
    /// <returns>  珂화 狗 UI 館絿都求.</returns>
    public async Task<TView> ShowAsync<TView>(ViewModelBase viewModel, Transform parent = null) where TView : UI_View
    {
        // 罐 천  ,   UI 트 爛求.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager]  琯 . path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        // parent null  첼 Push溝  확화爛求.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group  爛求.
        SetSortingGroupOrder(go, view is UI_Popup);

        // 韜쨔  爛求. (茱?AddRef)
        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    ///  타 UI_View 宙엽 琯構 환爛求.
    /// ResourceManagerEx  Object Pooling 湄 활爛求.
    /// </summary>
    /// <typeparam name="T"> UI 타見, UI_View 瞞 爛求.</typeparam>
    /// <param name="parent">UI 치 罐 Transform都求. null  타篤  湄 Root 絳求.</param>
    /// <returns> UI 館絿都求.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        // 罐 천  ,   UI 트 爛求.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager]  琯 . path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // parent null  첼 Push溝  확화爛求.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group  爛求.
        SetSortingGroupOrder(go, view is UI_Popup);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    ///  환퓸諍?캇 苛 UI_DontDestroyPopup 宙엽 琯構 ViewModel 爛求.
    /// </summary>
    /// <typeparam name="TView"> UI 타見, UI_DontDestroyPopup 瞞 爛求.</typeparam>
    /// <param name="viewModel">UI  ViewModel 館絿都求.</param>
    /// <param name="parent">UI 치 罐 Transform都求. null  DontDestroyRoot 羞?絳求.</param>
    /// <returns> UI 館絿都求.</returns>
    public async Task<TView> ShowDontDestroyAsync<TView>(ViewModelBase viewModel) where TView : UI_DontDestroyPopup
    {
        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager]  琯 . path: {path}");
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
    ///  환퓸諍?캇 苛 UI_DontDestroyPopup 宙엽 琯構 환爛求.
    ///  UI Popup Stack  , 瘤 怜餠 표천絳求.
    /// </summary>
    /// <typeparam name="T"> UI 타見, UI_DontDestroyPopup 瞞 爛求.</typeparam>
    /// <param name="parent">UI 치 罐 Transform都求. null  DontDestroyRoot 羞?絳求.</param>
    /// <returns> UI 館絿都求.</returns>
    public async Task<T> ShowDontDestroyAsync<T>() where T : UI_DontDestroyPopup
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager]  琯 . path: {path}");
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
    ///  UI_View 腑 Pool 환爛求.
    /// 鱇 ,  怜餠     笭求.
    /// </summary>
    /// <param name="view"> UI_View 館絿都求.</param>
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
                //    羞?("None")
                else
                {
                    Managers.Input.SwitchActionMap("None");
                }
            }
        }

        // UI 풀 환構킬 캇歐  ViewModel   求.
        //  ViewModel Release()    카트 構, 却 OnDispose() 호絳求.
        view.SetViewModel(null);

        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject SortingGroup 트 構 Sorting Order 爛求.
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
    ///   UI Root Transform 환爛求.  爛求.
    /// </summary>
    private Transform GetSceneRoot()
    {
        if (_sceneRoot == null)
        {
            GameObject rootGo = GameObject.Find("@UI_Root_Scene");

            // UI 트  , Canvas 迦 트 臼  爛求.
            if (rootGo == null)
            {
                rootGo = new GameObject { name = "@UI_Root_Scene" };
                rootGo.layer = LayerMask.NameToLayer("UI");

                Canvas canvas = rootGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                
                // ?꾩슜 UI 移대찓???ъ슜
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
    /// DontDestroy UI Root瑜?媛?몄삤嫄곕굹 ?앹꽦?⑸땲??
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
            
            // ?꾩슜 UI 移대찓???ъ슜
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
    /// UI ?꾩슜 移대찓?쇰? ?뺣낫?⑸땲??
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
        // ?꾩튂瑜?紐낆떆?곸쑝濡??ㅼ젙?섏뿬 ?ㅻ룞??諛⑹?
        cameraGo.transform.position = new Vector3(0, 0, -100);

        _uiCamera = cameraGo.GetOrAddComponent<Camera>();
        _uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI"); // UI ?덉씠?대쭔 ?뚮뜑留?
        _uiCamera.clearFlags = CameraClearFlags.Nothing;          // Overlay??Clear 遺덊븘??
        _uiCamera.orthographic = false;                           // ?먭렐 ?ъ쁺 ?쒖꽦??
        _uiCamera.fieldOfView = 60f;                              // ?쒖? FOV
        
        // URP Overlay Camera ?ㅼ젙
        var urpCameraData = _uiCamera.GetUniversalAdditionalCameraData();
        if (urpCameraData != null)
        {
            urpCameraData.renderType = CameraRenderType.Overlay;
        }
    }

    /// <summary>
    /// Main Camera??URP Camera Stack??UI Camera瑜??깅줉?⑸땲??
    /// </summary>
    /// <param name="mainCamera">Base Camera濡??ъ슜??Main Camera</param>
    private void RegisterUICameraToStack(Camera mainCamera)
    {
        if (_uiCamera == null) return;

        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (mainCameraData == null) return;
        
        // ?대? ?깅줉?섏뼱 ?덉쑝硫?以묐났 ?깅줉 諛⑹?
        if (mainCameraData.cameraStack.Contains(_uiCamera)) return;
        
        mainCameraData.cameraStack.Add(_uiCamera);
    }

    /// <summary>
    /// UI 타篤   罐 爛求.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// 恝  琯  호풔 遣트 湄冗?니?
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Camera.main != null)
        {
            // 硫붿씤 移대찓?쇨? UI ?덉씠?대? ?뚮뜑留곹븯吏 ?딅룄濡??ㅼ젙
            int uiLayerMask = 1 << LayerMask.NameToLayer("UI");
            Camera.main.cullingMask &= ~uiLayerMask;

            // URP Camera Stack??UI Camera ?깅줉
            RegisterUICameraToStack(Camera.main);
        }

        // UI 移대찓?쇰뒗 ??긽 議댁옱?댁빞 ??
        EnsureUICamera();

        Clear();
    }
}