using UnityEngine;

public abstract class AttributeEffectDefinition : ScriptableObject
{
    public abstract WeaponEffect CreateEffect(AttributeDefinition owner);
}
