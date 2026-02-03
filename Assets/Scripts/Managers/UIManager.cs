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
    /// UI Popup 관리를 위한 Sorting Order 입니다.
    /// </summary>
    private int _sortingOrder = 50;

    /// <summary>
    /// Sorting Group 증가용 order 단계
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

        // 초기 진입 시점에 Main Camera가 존재한다면 Stack에 등록합니다.
        // TestScene처럼 Awake/Start 시점에 이미 로드된 경우를 위해 필요합니다.
        if (Camera.main != null)
        {
            RegisterUICameraToStack(Camera.main);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        _popupStack.Clear();
        _sortingOrder = 10;
        _sceneRoot = null;
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    /// <summary>
    /// 특정 UI_View 팝업을 생성하고, ViewModel을 설정합니다.
    /// </summary>
    /// <typeparam name="TView"> 생성할 UI 타입, UI_View를 상속받아야 합니다.</typeparam>
    /// <param name="viewModel">UI에 연결할 ViewModel 객체입니다.</param>
    /// <param name="parent">UI가 배치될 부모 Transform입니다. null이면 기본 Scene별 Root를 사용합니다.</param>
    /// <returns> 생성/활성화 된 UI 객체입니다.</returns>
    public async Task<TView> ShowAsync<TView>(ViewModelBase viewModel, Transform parent = null) where TView : UI_View
    {
        // 부모가 주어지지 않으면, 씬별 UI 루트를 찾습니다.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        // parent가 null이면 Popup Stack에 Push하고 ActionMap을 전환합니다.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group 순서를 설정합니다.
        SetSortingGroupOrder(go, view is UI_Popup);

        // 뷰모델을 설정합니다. (내부에서 AddRef)
        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 특정 UI_View 팝업을 생성하고 반환합니다.
    /// ResourceManagerEx를 통해 Object Pooling이 자동으로 적용됩니다.
    /// </summary>
    /// <typeparam name="T"> 생성할 UI 타입, UI_View를 상속받아야 합니다.</typeparam>
    /// <param name="parent">UI가 배치될 부모 Transform입니다. null이면 기본 Scene별 Root를 사용합니다.</param>
    /// <returns> 생성된 UI 객체입니다.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        // 부모가 주어지지 않으면, 씬별 UI 루트를 찾습니다.
        Transform root = parent == null ? GetSceneRoot() : parent;

        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: root);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        // parent가 null이면 Popup Stack에 Push하고 ActionMap을 전환합니다.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group 순서를 설정합니다.
        SetSortingGroupOrder(go, view is UI_Popup);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 씬이 변경되어도 유지되는 UI_DontDestroyPopup 팝업을 생성하고 ViewModel을 설정합니다.
    /// </summary>
    /// <typeparam name="TView"> 생성할 UI 타입, UI_DontDestroyPopup을 상속받아야 합니다.</typeparam>
    /// <param name="viewModel">UI에 연결할 ViewModel 객체입니다.</param>
    /// <param name="parent">UI가 배치될 부모 Transform입니다. null이면 DontDestroyRoot를 사용합니다.</param>
    /// <returns> 생성된 UI 객체입니다.</returns>
    public async Task<TView> ShowDontDestroyAsync<TView>(ViewModelBase viewModel) where TView : UI_DontDestroyPopup
    {
        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
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
    /// 씬이 변경되어도 유지되는 UI_DontDestroyPopup 팝업을 생성하고 반환합니다.
    /// 일반 UI Popup Stack에 들어가지 않으며, 보통 최상위에 표시됩니다.
    /// </summary>
    /// <typeparam name="T"> 생성할 UI 타입, UI_DontDestroyPopup을 상속받아야 합니다.</typeparam>
    /// <param name="parent">UI가 배치될 부모 Transform입니다. null이면 DontDestroyRoot를 사용합니다.</param>
    /// <returns> 생성된 UI 객체입니다.</returns>
    public async Task<T> ShowDontDestroyAsync<T>() where T : UI_DontDestroyPopup
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: GetDontDestroyRoot());
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
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
    /// UI_View를 닫고 Pool에 반환합니다.
    /// 팝업인 경우, 스택 관리 및 오더 정리도 수행합니다.
    /// </summary>
    /// <param name="view"> 닫을 UI_View 객체입니다.</param>
    public void Close(UI_View view)
    {
        if (view == null) return;

        if (view is UI_Popup popup && view is not UI_DontDestroyPopup)
        {
            if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                _sortingOrder -= ORDER_STEP;

                // 다음 Popup의 ActionMapKey로 전환
                if (_popupStack.Count > 0)
                {
                    var nextPopup = _popupStack.Peek();
                    Managers.Input.SwitchActionMap(nextPopup.ActionMapKey);
                }
                // 팝업이 없으면 None("None")
                else
                {
                    Managers.Input.SwitchActionMap("None");
                }
            }
        }

        // UI를 풀에 반환하거나 파괴하기 전에 ViewModel 연결을 끊습니다.
        // 기존 ViewModel의 Release()를 호출하여 참조 카운트를 감소시키고, 필요 시 OnDispose()를 호출합니다.
        view.SetViewModel(null);

        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject에 SortingGroup을 세팅하고 Sorting Order를 지정합니다.
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
            // sortingOrder 미사용 (UI_Popup 아님) 시 sortingOrder 0
            sortingGroup.sortingOrder = 0;
        }
    }

    /// <summary>
    /// 현재 씬의 UI Root Transform을 반환합니다. 없으면 생성합니다.
    /// </summary>
    private Transform GetSceneRoot()
    {
        if (_sceneRoot == null)
        {
            GameObject rootGo = GameObject.Find("@UI_Root_Scene");

            // UI 루트 생성 시, Canvas와 필수 컴포넌트를 부착합니다.
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
        _uiCamera.clearFlags = CameraClearFlags.Nothing;          // Overlay이므로 Clear 불필요
        _uiCamera.orthographic = false;                           // 원근 투영 활성화 (Perspective)
        _uiCamera.fieldOfView = 60f;                              // 수직 FOV

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
    /// <param name="mainCamera">Base Camera로 사용될 Main Camera</param>
    private void RegisterUICameraToStack(Camera mainCamera)
    {
        if (_uiCamera == null) return;

        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        if (mainCameraData == null) return;

        // 이미 등록되어 있다면 중복 등록 방지
        if (mainCameraData.cameraStack.Contains(_uiCamera)) return;

        mainCameraData.cameraStack.Add(_uiCamera);
    }

    /// <summary>
    /// UI 타입에 따른 프리팹 경로를 반환합니다.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// 씬이 로드될 때 호출되어 카메라 스택을 재설정합니다.
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

        GetSceneRoot();
    }
}