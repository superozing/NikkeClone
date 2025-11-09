
interface IManagerBase
{
    public void Init();
    public void Start();
    public void Update();
    public void Clear();
    public eManagerType ManagerType { get; }
}