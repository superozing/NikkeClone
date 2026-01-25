using UI;

/// <summary>
/// 적정 사거리 정보 ViewModel입니다.
/// 스쿼드 니케들의 무기 타입에 따른 사거리별 분류를 관리합니다.
/// </summary>
public class StageRangeInfoViewModel : ViewModelBase
{
    // --- 사거리별 니케 수 ---
    /// <summary>
    /// Near 범위 니케 수입니다. (SG, SMG)
    /// </summary>
    public ReactiveProperty<int> NearCount { get; private set; } = new(0);

    /// <summary>
    /// Mid 범위 니케 수입니다. (AR, MG)
    /// </summary>
    public ReactiveProperty<int> MidCount { get; private set; } = new(0);

    /// <summary>
    /// Far 범위 니케 수입니다. (SR)
    /// </summary>
    public ReactiveProperty<int> FarCount { get; private set; } = new(0);

    /// <summary>
    /// 스쿼드 정보를 기반으로 사거리별 니케 수를 계산합니다.
    /// 무기 타입에 따라 Near/Mid/Far 범위로 분류합니다.
    /// </summary>
    /// <param name="squadNikkes">현재 스쿼드의 NikkeIconViewModel 배열</param>
    public void SetData(NikkeIconViewModel[] squadNikkes)
    {
        int near = 0, mid = 0, far = 0;

        if (squadNikkes != null)
        {
            foreach (var nikke in squadNikkes)
            {
                if (nikke == null || nikke.IsSlotEmpty) continue;

                switch (nikke.WeaponType)
                {
                    case eNikkeWeapon.SG:
                    case eNikkeWeapon.SMG:
                        near++;
                        break;
                    case eNikkeWeapon.AR:
                    case eNikkeWeapon.MG:
                        mid++;
                        break;
                    case eNikkeWeapon.SR:
                        far++;
                        break;
                }
            }
        }

        NearCount.Value = near;
        MidCount.Value = mid;
        FarCount.Value = far;
    }

    protected override void OnDispose()
    {
        // ReactiveProperty 정리는 ViewModelBase에서 처리
    }
}
