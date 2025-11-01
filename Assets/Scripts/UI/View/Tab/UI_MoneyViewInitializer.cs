using UnityEngine;

public class UI_MoneyViewInitializer : MonoBehaviour
{
    [SerializeField] private UI_Money _moneyView;
    private MoneyViewModel _viewModel;

    private void Awake()
    {
        _moneyView.SetViewModel(_viewModel = new MoneyViewModel());
    }

    private void OnDestroy()
    {
        _viewModel?.Dispose();
    }
}