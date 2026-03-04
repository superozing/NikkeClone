using System;

[Serializable]
public struct StatGroup
{
    public float HP;
    public float Attack;
    public float Defense;
    public float CriticalRate;
    public float CriticalDamage;

    public static StatGroup operator +(StatGroup a, StatGroup b)
    {
        return new StatGroup
        {
            HP = a.HP + b.HP,
            Attack = a.Attack + b.Attack,
            Defense = a.Defense + b.Defense,
            CriticalRate = a.CriticalRate + b.CriticalRate,
            CriticalDamage = a.CriticalDamage + b.CriticalDamage
        };
    }

    public static StatGroup operator -(StatGroup a, StatGroup b)
    {
        return new StatGroup
        {
            HP = a.HP - b.HP,
            Attack = a.Attack - b.Attack,
            Defense = a.Defense - b.Defense,
            CriticalRate = a.CriticalRate - b.CriticalRate,
            CriticalDamage = a.CriticalDamage - b.CriticalDamage
        };
    }
}
