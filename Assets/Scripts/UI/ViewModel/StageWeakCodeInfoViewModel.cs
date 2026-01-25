using UI;
using UnityEngine;

/// <summary>
/// 약점 코드 정보 ViewModel입니다.
/// 스테이지의 약점 코드와 매칭되는 스쿼드 니케 수를 관리합니다.
/// Implements Section 1: UI_StageWeakCodeInfo from UI_StageSubUI_Design.md
/// </summary>
public class StageWeakCodeInfoViewModel : ViewModelBase
{
    // --- 약점 코드 정보 ---
    /// <summary>
    /// 스테이지의 약점 코드입니다.
    /// </summary>
    public eNikkeCode WeaknessCode { get; private set; }

    /// <summary>
    /// 약점 코드에 해당하는 속성을 가진 스쿼드 니케 수입니다. (0~5)
    /// </summary>
    public ReactiveProperty<int> MatchedCount { get; private set; } = new(0);

    /// <summary>
    /// 약점 코드 아이콘 스프라이트입니다.
    /// </summary>
    public ReactiveProperty<Sprite> WeaknessCodeSprite { get; private set; } = new();

    /// <summary>
    /// 약점 코드 및 스쿼드 정보를 설정합니다.
    /// 스쿼드 내 니케 중 약점 코드와 동일한 속성을 가진 니케 수를 계산합니다.
    /// </summary>
    /// <param name="weaknessCode">스테이지의 약점 코드 (StageGameData.weaknessCode)</param>
    /// <param name="squadNikkes">현재 스쿼드의 NikkeIconViewModel 배열</param>
    public async void SetData(int weaknessCode, NikkeIconViewModel[] squadNikkes)
    {
        WeaknessCode = (eNikkeCode)weaknessCode;

        // 1. 약점코드 스프라이트 로드 (Addressables)
        string codePath = GetCodeSpritePath(WeaknessCode);
        if (!string.IsNullOrEmpty(codePath))
        {
            WeaknessCodeSprite.Value = await Managers.Resource.LoadAsync<Sprite>(codePath);
        }

        // 2. 스쿼드 내 매칭 니케 수 계산
        int count = 0;
        if (squadNikkes != null)
        {
            foreach (var nikke in squadNikkes)
            {
                if (nikke == null || nikke.IsSlotEmpty) continue;
                if (nikke.CodeType == WeaknessCode)
                    count++;
            }
        }
        MatchedCount.Value = count;
    }

    /// <summary>
    /// eNikkeCode에 해당하는 스프라이트 경로를 반환합니다.
    /// </summary>
    private string GetCodeSpritePath(eNikkeCode code)
    {
        if (code == eNikkeCode.None) return null;

        // 코드명을 한글로 변환하여 경로 생성
        string codeName = code switch
        {
            eNikkeCode.Fire => "작열",
            eNikkeCode.Water => "수냉",
            eNikkeCode.Wind => "풍압",
            eNikkeCode.Electric => "전격",
            eNikkeCode.Iron => "철갑",
            _ => null
        };

        if (codeName == null) return null;
        return $"Assets/Textures/Icon/Code/{codeName}";
    }

    protected override void OnDispose()
    {
        // ReactiveProperty 정리는 ViewModelBase에서 처리
    }
}
