using System.Collections.Generic;
using System.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.UI;

    private readonly Stack<UI_Popup> _popupStack = new();
    private Transform _sceneRoot;
    private Transform _dontDestroyRoot;

    /// <summary>
    /// UI Popup에 순차적으로 부여될 Sorting Order 값입니다.
    /// </summary>
    private int _sortingOrder = 50;

    /// <summary>
    /// Sorting Group 간의 order 간격
    /// </summary>
    private const int ORDER_STEP = 10;

    public void Init()
    {
        GameObject dontDestroyGo = GameObject.Find("@UI_Root_DontDestroy") ?? new GameObject { name = "@UI_Root_DontDestroy" };
        Object.DontDestroyOnLoad(dontDestroyGo);
        _dontDestroyRoot = dontDestroyGo.transform;

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject { name = "@EventSystem" };
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();
            Object.DontDestroyOnLoad(eventSystemGo);
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
    /// 지정된 타입의 UI_View를 비동기적으로 로드하고, 제공된 ViewModel을 주입합니다.
    /// </summary>
    /// <typeparam name="TView">생성할 UI의 타입이며, UI_View를 상속해야 합니다.</typeparam>
    /// <param name="viewModel">UI에 주입할 ViewModel 인스턴스입니다.</param>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 타입에 따라 자동으로 Root가 결정됩니다.</param>
    /// <returns>생성 및 초기화가 완료된 UI의 인스턴스입니다.</returns>
    public async Task<TView> ShowAsync<TView>(IViewModel viewModel, Transform parent = null) where TView : UI_View
    {
        // 부모가 명시되지 않은 경우, 현재 씬의 UI 루트를 사용합니다.
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

        // parent가 null일 때만 스택에 Push하도록 로직을 명확화합니다.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group의 순서를 설정합니다.
        SetSortingGroupOrder(go, view is UI_Popup);

        // 입력받은 뷰모델을 세팅합니다.
        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 지정된 타입의 UI_View를 비동기적으로 로드하고 반환합니다.
    /// ResourceManagerEx를 통해 Object Pooling을 자동으로 활용합니다.
    /// </summary>
    /// <typeparam name="T">생성할 UI의 타입이며, UI_View를 상속해야 합니다.</typeparam>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 타입에 따라 자동으로 Root가 결정됩니다.</param>
    /// <returns>생성된 UI의 인스턴스입니다.</returns>
    public async Task<T> ShowAsync<T>(Transform parent = null) where T : UI_View
    {
        // 부모가 명시되지 않은 경우, 현재 씬의 UI 루트를 사용합니다.
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

        // parent가 null일 때만 스택에 Push하도록 로직을 명확화합니다.
        if (parent == null && view is UI_Popup popup)
        {
            _popupStack.Push(popup);
            Managers.Input.SwitchActionMap(popup.ActionMapKey);
        }

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        // Sorting Group의 순서를 설정합니다.
        SetSortingGroupOrder(go, view is UI_Popup);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 씬이 전환되어도 파괴되지 않는 UI_DontDestroyPopup을 비동기적으로 로드하고 ViewModel을 주입합니다.
    /// </summary>
    /// <typeparam name="TView">생성할 UI의 타입이며, UI_DontDestroyPopup을 상속해야 합니다.</typeparam>
    /// <param name="viewModel">UI에 주입할 ViewModel 인스턴스입니다.</param>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 DontDestroyRoot가 기본값으로 사용됩니다.</param>
    /// <returns>생성된 UI의 인스턴스입니다.</returns>
    public async Task<TView> ShowDontDestroyAsync<TView>(IViewModel viewModel, Transform parent = null) where TView : UI_DontDestroyPopup
    {
        string prefabName = typeof(TView).Name;
        string path = GetPrefabPath<TView>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent != null ? parent : _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        TView view = go.GetOrAddComponent<TView>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        view.SetViewModel(viewModel);

        view.gameObject.SetActive(true);
        return view;
    }

    /// <summary>
    /// 씬이 전환되어도 파괴되지 않는 UI_DontDestroyPopup을 비동기적으로 로드하고 반환합니다.
    /// 이 UI는 Popup Stack으로 관리되지 않으며, 항상 최상단에 표시됩니다.
    /// </summary>
    /// <typeparam name="T">생성할 UI의 타입이며, UI_DontDestroyPopup을 상속해야 합니다.</typeparam>
    /// <param name="parent">UI가 위치할 부모 Transform입니다. null일 경우 DontDestroyRoot가 기본값으로 사용됩니다.</param>
    /// <returns>생성된 UI의 인스턴스입니다.</returns>
    public async Task<T> ShowDontDestroyAsync<T>(Transform parent = null) where T : UI_DontDestroyPopup
    {
        string prefabName = typeof(T).Name;
        string path = GetPrefabPath<T>(prefabName);

        GameObject go = await Managers.Resource.InstantiateAsync(path, parent: parent != null ? parent : _dontDestroyRoot);
        if (go == null)
        {
            Debug.LogError($"[UIManager] 프리팹 로드 실패. path: {path}");
            return null;
        }

        T view = go.GetOrAddComponent<T>();

        var rectTransform = view.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;

        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 999999;

        view.gameObject.SetActive(true);
        return view;
    }


    /// <summary>
    /// 지정된 UI_View를 닫고 Pool에 반환합니다.
    /// 팝업의 경우, 스택의 최상단에 있을 때만 닫을 수 있습니다.
    /// </summary>
    /// <param name="view">닫을 UI_View 인스턴스입니다.</param>
    public void Close(UI_View view)
    {
        if (view == null) return;

        if (view is UI_Popup popup && view is not UI_DontDestroyPopup)
        {
            if (_popupStack.Count > 0 && _popupStack.Peek() == popup)
            {
                _popupStack.Pop();
                _sortingOrder -= ORDER_STEP;

                // 스택 상단 Popup의 ActionMapKey 세팅
                if (_popupStack.Count > 0)
                {
                    var nextPopup = _popupStack.Peek();
                    Managers.Input.SwitchActionMap(nextPopup.ActionMapKey);
                }
                // 스택이 빈 경우 기본 세팅("None")
                else
                {
                    Managers.Input.SwitchActionMap("None");
                }
            }
        }

        Managers.Resource.Destroy(view.gameObject);
    }

    /// <summary>
    /// UI GameObject에 SortingGroup 컴포넌트를 설정하고 Sorting Order를 지정합니다.
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
            // sortingOrder를 사용하지 않을 경우(UI_Popup을 상속할 경우) sortingOrder를 사용하지 않음
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

            //씬에 UI 루트가 없을 경우, Canvas와 필수 컴포넌트를 포함하여 새로 생성합니다.
            if (rootGo == null)
            {
                rootGo = new GameObject { name = "@UI_Root_Scene" };
                rootGo.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
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
    /// UI 타입에 따라 프리팹 경로를 결정합니다.
    /// </summary>
    private string GetPrefabPath<T>(string prefabName) where T : UI_View
    {
        string folder = typeof(UI_Popup).IsAssignableFrom(typeof(T)) ? "Popup" : "View";
        return $"UI/{folder}/{prefabName}";
    }

    /// <summary>
    /// 새로운 씬이 로드될 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Clear();

        // 나중에 씬 마다 필요한 동작이 있다면 추가하면 좋겠죠?
    }
}