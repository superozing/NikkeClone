/// <summary>
/// 데이터와 무기 타입을 기반으로 적절한 IWeapon 구체 클래스를 생성하는 팩토리입니다.
/// </summary>
public static class WeaponFactory
{
    /// <summary>
    /// 무기 타입에 맞는 구체 클래스를 생성 반환합니다.
    /// Caller: CombatSystem.InitializeNikkesAsync()
    /// </summary>
    public static IWeapon CreateWeapon(WeaponData data, eNikkeWeapon weaponType)
    {
        return weaponType switch
        {
            eNikkeWeapon.AR => new ARWeapon(data),
            eNikkeWeapon.SMG => new SMGWeapon(data),
            eNikkeWeapon.MG => new MGWeapon(data),
            eNikkeWeapon.SG => new SGWeapon(data),
            eNikkeWeapon.SR => new SRWeapon(data),
            eNikkeWeapon.RL => new RLWeapon(data),
            _ => new ARWeapon(data), // Fallback
            // TODO: 특수한 무기의 경우 처리 필요(라피나 신데렐라 같은..)
        };
    }
}
