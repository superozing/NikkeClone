using System;
using UnityEngine;
using NikkeClone.Utils;

[Serializable]
public struct EffectData
{
    public string EffectName;
    public string Description;
    public float Duration;
    public StatGroup StatModifiers;
    public eStatModifierType ModifierType;
    public Sprite Icon;
}
