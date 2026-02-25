using UnityEngine;

public class CombatResultDefeatPopupViewModel : ViewModelBase
{
    public void OnRetryClicked()
    {
        Debug.Log("[CombatScene] 재도전 버튼(아직 구현되지 않았어요)");
    }

    public void OnUpgradeClicked()
    {
        // TODO: MainScene 로드 후 니케 탭으로 이동하도록 해야 해요. 아직 그런 방법이 구현되지 않았어요.
        Debug.Log("[CombatScene] 니케 강화 버튼(니케 탭으로 이동하는 로직이 아직 구현되지 않았어요)");
        Managers.Scene.LoadSceneAsync(eSceneType.MainScene);
    }

    public void OnExitClicked()
    {
        Managers.Scene.LoadSceneAsync(eSceneType.CampaignScene);
    }
}
