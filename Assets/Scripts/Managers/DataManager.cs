using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class DataManager : IManagerBase
{
    public eManagerType ManagerType { get; } = eManagerType.Data;

    // --- 사용자 데이터 (쓰기 가능) ---
    private const string UserDataPath = "UserData.json";
    public UserDataModel UserData { get; private set; }

    // --- 게임 데이터 (읽기 전용) ---
    private readonly Dictionary<Type, object> _dataTables = new();

    // Why: Newtonsoft.Json 사용 시 ReactiveProperty<T>를 올바르게 처리하기 위해
    // 사용자 정의 컨버터를 포함하는 JsonSerializerSettings를 미리 정의해 둡니다.
    // 이렇게 하면 직렬화/역직렬화가 필요할 때마다 설정을 반복해서 생성할 필요가 없어 효율적입니다.
    private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { new ReactivePropertyConverter() }
    };

    public void Init()
    {
        LoadUserData();
        Debug.Log($"{ManagerType} Manager Init 합니다.");
    }

    public void Update() { }

    public void Clear()
    {
        // 씬에서 사용하던 게임 데이터 클리어
        _dataTables.Clear();
        Debug.Log($"{ManagerType} Manager Clear 합니다.");
    }

    // ================================================================================

    #region Game Data (Read-only) Methods

    /// <summary>
    /// SceneManagerEx가 씬 전환 시 호출하는 함수입니다.
    /// 씬에 필요한 JSON 파일 목록을 받아와, ResourceManagerEx에 로드를 요청하고 파싱합니다.
    /// </summary>
    /// <param name="fileNames">해당 씬의 IScene 구현체에 정의된 JSON 파일 이름 목록입니다.</param>
    public async Task LoadDataForSceneAsync(List<string> fileNames)
    {
        List<Task> loadingTasks = new();
        foreach (var fileName in fileNames)
        {
            // switch 문을 통해 파일 이름(string)과 실제 데이터 타입(class)을 명확하게 연결합니다.
            switch (fileName)
            {
                case "NikkeGameData.json":
                    loadingTasks.Add(LoadJsonAsync<NikkeGameData>(fileName));
                    break;
                case "ItemGameData.json":
                    loadingTasks.Add(LoadJsonAsync<ItemGameData>(fileName));
                    break;

                // 새로운 GameData를 추가할 경우 여기에 case 구문을 추가

                default:
                    Debug.LogWarning($"[DataManager] 로드 규칙이 정의되지 않은 파일입니다: {fileName}");
                    break;
            }
        }
        // 모든 파일 로딩과 파싱이 병렬로 처리되고, 완료될 때까지 기다립니다.
        await Task.WhenAll(loadingTasks);
    }

    /// <summary>
    /// 지정된 타입의 데이터 테이블 전체를 읽기 전용 딕셔너리로 반환합니다.
    /// </summary>
    public IReadOnlyDictionary<int, T> GetTable<T>() where T : IDataId
    {
        if (_dataTables.TryGetValue(typeof(T), out object table))
            return table as IReadOnlyDictionary<int, T>;

        Debug.LogError($"[DataManager] GetTable<{typeof(T).Name}>(): 테이블을 찾을 수 없습니다. 씬의 RequiredDataFiles 목록에 해당 JSON 파일이 정의되어 있는지 확인하세요.");
        return null;
    }

    /// <summary>
    /// 지정된 ID를 가진 특정 게임 데이터를 가져옵니다.
    /// </summary>
    public T Get<T>(int id) where T : IDataId
    {
        var table = GetTable<T>();
        if (table != null && table.TryGetValue(id, out T data))
            return data;

        Debug.LogWarning($"[DataManager] Get<{typeof(T).Name}>(): ID({id})에 해당하는 데이터를 찾을 수 없습니다.");
        return default;
    }

    /// <summary>
    /// ResourceManagerEx에 JSON 파일 로드를 요청하고, 받아온 TextAsset을 파싱하여 딕셔너리에 저장합니다.
    /// </summary>
    private async Task LoadJsonAsync<T>(string key) where T : IDataId
    {
        // Why: 실제 파일 로딩은 ResourceManagerEx의 책입입니다. DataManager는 이 결과를 받아 파싱만 담당합니다.
        TextAsset textAsset = await Managers.Resource.LoadAsync<TextAsset>(key);

        if (textAsset != null)
        {
            // Why: JSON 파일이 Dictionary<string, T> 형태로 되어있다고 가정하고 직접 파싱합니다.
            // 이렇게 하면 List<T>로 변환 후 다시 ToDictionary()를 호출하는 중간 과정이 생략되어 더 효율적입니다.
            // JSON의 키는 문자열이므로, 먼저 Dictionary<string, T>로 받은 후, int 키를 사용하는 최종 Dictionary로 변환합니다.
            var rawDict = JsonConvert.DeserializeObject<Dictionary<string, T>>(textAsset.text);
            var dict = rawDict.ToDictionary(pair => int.Parse(pair.Key), pair => pair.Value);

            _dataTables.Add(typeof(T), dict);
            Debug.Log($"[DataManager] JSON 데이터 파싱 성공 (Newtonsoft.Json): {key}");
        }
        else
        {
            Debug.LogError($"[DataManager] JSON 파일 로드에 실패했습니다. ResourceManagerEx가 파일을 찾지 못했습니다: {key}");
        }
    }

    #endregion

    // ================================================================================

    #region User Data (Read-Write) Methods

    /// <summary>
    /// 현재 사용자 데이터를 JSON 파일로 저장합니다. 게임 종료 또는 특정 저장 시점에 호출합니다.
    /// </summary>
    public void SaveUserData()
    {
        if (UserData == null)
        {
            Debug.LogError("[DataManager] 저장할 UserData가 없습니다. LoadUserData가 실패했거나, 아직 데이터가 설정되지 않았습니다.");
            return;
        }

        string savePath = Path.Combine(Application.persistentDataPath, UserDataPath);

        // Formatting.Indented 옵션은 JSON을 사람이 읽기 쉽게 들여쓰기하여 저장합니다.
        // _jsonSettings를 전달하여 ReactiveProperty<T>가 올바르게 직렬화되도록 합니다.
        string json = JsonConvert.SerializeObject(UserData, Formatting.Indented, _jsonSettings);

        File.WriteAllText(savePath, json);
        Debug.Log($"[DataManager] 유저 데이터 저장 완료 (Newtonsoft.Json): {savePath}");
    }

    /// <summary>
    /// 파일에서 사용자 데이터를 로드합니다. Init()에서만 호출됩니다.
    /// </summary>
    private void LoadUserData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, UserDataPath);
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);

            // _jsonSettings를 전달하여 JSON의 값을 ReactiveProperty<T> 객체로 올바르게 변환합니다.
            UserData = JsonConvert.DeserializeObject<UserDataModel>(json, _jsonSettings);

            Debug.Log($"[DataManager] 유저 데이터 로드 완료: {savePath}");
        }
        else
        {
            UserData = null;
            Debug.LogWarning($"[DataManager] 유저 데이터 파일({UserDataPath})을 찾을 수 없습니다. UserData가 null입니다.");
        }
    }

    #endregion
}