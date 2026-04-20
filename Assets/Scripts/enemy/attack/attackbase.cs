using UnityEngine;
using System.Collections;

public abstract class AttackPattern : ScriptableObject
{
    public abstract IEnumerator Execute(AttackContext context);
}
